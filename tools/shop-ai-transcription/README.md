# Shop AI Transcription Service

Local speech-to-text microservice for the Shop AI Assistant, built on
[faster-whisper](https://github.com/SYSTRAN/faster-whisper). It is a plain FastAPI app - no
database, no business logic, no knowledge of tenants or shop data. It only converts an uploaded
audio clip into text and returns it.

The .NET backend (`IShopSpeechToTextClient`) calls this service server-to-server; it is not meant
to be exposed directly to browsers.

## Endpoints

- `POST /transcribe` - multipart form with an `audio` file field (`.webm`, `.wav`, `.mp3`, `.m4a`,
  `.ogg`) and an optional `language` field (e.g. `en`, `ur`). Returns:

  ```json
  {
    "text": "Ali ke naam se customer add karo",
    "language": "ur",
    "languageProbability": 0.94,
    "durationSeconds": 4.2
  }
  ```

- `GET /health` - liveness/readiness probe used by the backend's health check.

## Local setup (Windows)

```powershell
python -m venv .venv
.venv\Scripts\activate
pip install -r requirements.txt
uvicorn app:app --host 0.0.0.0 --port 8001
```

## Local setup (macOS / Linux)

```bash
python -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
uvicorn app:app --host 0.0.0.0 --port 8001
```

The Whisper model itself is **not** downloaded at startup - it is lazily loaded on the first
`/transcribe` request (via `faster_whisper.WhisperModel(...)`), and cached by `faster-whisper`
in the usual Hugging Face cache directory afterward. The first request after starting the service
will therefore be noticeably slower than subsequent ones.

## Configuration (environment variables)

| Variable                | Default | Matches `ShopAiSpeech` config |
|--------------------------|---------|--------------------------------|
| `WHISPER_MODEL`          | `small` | `Model`                        |
| `WHISPER_DEVICE`         | `cpu`   | `Device`                       |
| `WHISPER_COMPUTE_TYPE`   | `int8`  | `ComputeType`                  |
| `MAX_AUDIO_SIZE_MB`      | `15`    | `MaximumAudioSizeMb`            |
| `MAX_DURATION_SECONDS`   | `120`   | `MaximumDurationSeconds`        |

Keep these in sync with the `ShopAiSpeech` section of the .NET backend's `appsettings.json` -
the backend enforces its own copy of the size/duration limits before it ever calls this service,
but this service enforces them independently too (defense in depth).

## Docker

```bash
docker build -t shop-ai-transcription .
docker run -p 8001:8001 shop-ai-transcription
```

For GPU acceleration, set `WHISPER_DEVICE=cuda` and `WHISPER_COMPUTE_TYPE=float16` and use a
CUDA-enabled base image with the appropriate NVIDIA container runtime - not covered by the
default Dockerfile here, which targets CPU-only inference.

## Known limitations

- **CPU inference is slow.** The `small` model on CPU with `int8` compute is usable for short
  shopkeeper commands (a few seconds of audio) but will feel sluggish for anything longer.
  `tiny` or `base` trade accuracy for speed if `small` feels too slow on your hardware.
- **Roman Urdu is not a distinct Whisper language.** Whisper transcribes spoken Roman-Urdu-style
  speech phonetically into whatever script it predicts (often Urdu script, sometimes Latin script
  mixed with English) - the "Roman Urdu" language label used elsewhere in this feature is applied
  by the Ollama command parser reading the resulting *text*, not by this speech-to-text step.
- **No GPU by default.** The default Dockerfile and instructions target CPU-only inference for
  simplicity; see the Docker section above for GPU notes.
- **Single-process, single-model.** This service holds one loaded model in memory and processes
  requests sequentially per worker - it is not tuned for high concurrent voice traffic.
