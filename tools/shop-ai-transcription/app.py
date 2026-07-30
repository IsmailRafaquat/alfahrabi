"""
Local speech-to-text service for the Shop AI Assistant, backed by faster-whisper.

Security/scope notes (matching the .NET backend's expectations):
- This service knows nothing about tenants, users, customers, or any shop data. It converts
  audio to text and nothing else.
- The uploaded file's original name is never trusted or used to build a path - a random
  temporary filename is generated for every request.
- The temporary audio file is always deleted immediately after transcription (success or
  failure) and audio is never persisted to disk beyond that.
- Requests are only ever expected from the trusted .NET backend on the local network, not
  directly from a browser - there is no auth layer here by design (add one before exposing this
  beyond localhost / a private network).
"""

import os
import tempfile
import time
import uuid
from typing import Optional

from fastapi import FastAPI, File, Form, HTTPException, UploadFile
from fastapi.responses import JSONResponse

MODEL_SIZE = os.environ.get("WHISPER_MODEL", "small")
DEVICE = os.environ.get("WHISPER_DEVICE", "cpu")
COMPUTE_TYPE = os.environ.get("WHISPER_COMPUTE_TYPE", "int8")
MAX_AUDIO_SIZE_MB = float(os.environ.get("MAX_AUDIO_SIZE_MB", "15"))
MAX_DURATION_SECONDS = float(os.environ.get("MAX_DURATION_SECONDS", "120"))

ALLOWED_EXTENSIONS = {".webm", ".wav", ".mp3", ".m4a", ".ogg"}
ALLOWED_CONTENT_TYPE_PREFIXES = (
    "audio/webm", "audio/wav", "audio/x-wav", "audio/wave",
    "audio/mpeg", "audio/mp3", "audio/mp4", "audio/m4a", "audio/x-m4a", "audio/ogg",
)

app = FastAPI(title="Shop AI Transcription Service", version="1.0.0")

_model = None


def get_model():
    """Loaded lazily on first request, not at import/startup time - so `uvicorn app:app`
    starts instantly and the (larger) model download/load only happens when actually needed."""
    global _model
    if _model is None:
        from faster_whisper import WhisperModel
        _model = WhisperModel(MODEL_SIZE, device=DEVICE, compute_type=COMPUTE_TYPE)
    return _model


@app.get("/health")
def health():
    return {"status": "ok", "model": MODEL_SIZE, "device": DEVICE, "computeType": COMPUTE_TYPE}


@app.post("/transcribe")
async def transcribe(audio: UploadFile = File(...), language: Optional[str] = Form(default=None)):
    original_extension = os.path.splitext(audio.filename or "")[1].lower()
    if original_extension not in ALLOWED_EXTENSIONS:
        raise HTTPException(status_code=400, detail=f"Unsupported audio extension '{original_extension}'.")

    content_type = (audio.content_type or "").lower()
    if not content_type.startswith(ALLOWED_CONTENT_TYPE_PREFIXES):
        raise HTTPException(status_code=400, detail=f"Unsupported content type '{content_type}'.")

    data = await audio.read()
    if not data:
        raise HTTPException(status_code=400, detail="Audio file is empty.")

    size_mb = len(data) / (1024 * 1024)
    if size_mb > MAX_AUDIO_SIZE_MB:
        raise HTTPException(status_code=413, detail=f"Audio file exceeds the {MAX_AUDIO_SIZE_MB} MB limit.")

    # Random, server-chosen file name - the browser/client-supplied file name is used only to
    # read the extension above, never to build a path.
    temp_path = os.path.join(tempfile.gettempdir(), f"shop-ai-{uuid.uuid4().hex}{original_extension}")

    try:
        with open(temp_path, "wb") as f:
            f.write(data)

        model = get_model()
        started_at = time.monotonic()
        segments, info = model.transcribe(temp_path, language=language, vad_filter=True)
        text = "".join(segment.text for segment in segments).strip()
        elapsed = time.monotonic() - started_at

        duration_seconds = float(info.duration) if info and info.duration else elapsed

        if duration_seconds > MAX_DURATION_SECONDS:
            raise HTTPException(status_code=413, detail=f"Recording exceeds the {MAX_DURATION_SECONDS}s limit.")

        return JSONResponse({
            "text": text,
            "language": info.language if info else None,
            "languageProbability": float(info.language_probability) if info and info.language_probability else 0.0,
            "durationSeconds": duration_seconds,
        })
    finally:
        # Never store audio by default - delete the temp file immediately, success or failure.
        if os.path.exists(temp_path):
            os.remove(temp_path)
