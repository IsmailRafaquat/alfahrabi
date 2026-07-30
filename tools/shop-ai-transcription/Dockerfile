FROM python:3.11-slim

WORKDIR /app

# ffmpeg is required by faster-whisper/ctranslate2's audio decoding path for most container formats.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY app.py .

ENV WHISPER_MODEL=small \
    WHISPER_DEVICE=cpu \
    WHISPER_COMPUTE_TYPE=int8 \
    MAX_AUDIO_SIZE_MB=15 \
    MAX_DURATION_SECONDS=120

EXPOSE 8001

# The model is downloaded on first request (see app.py's lazy get_model()), not at image build
# or container start time - matches "do not automatically download AI models during startup".
CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8001"]
