FROM python:3.11-slim

WORKDIR /app

COPY server/requirements.txt /app/server/requirements.txt
RUN python -m pip install --no-cache-dir --upgrade "pip>=26.1.2" "setuptools>=83.0.0" && \
    python -m pip install --no-cache-dir -r /app/server/requirements.txt && \
    python -m pip check

COPY server /app/server

ENV PYTHONUNBUFFERED=1

CMD ["uvicorn", "server.app:app", "--host", "0.0.0.0", "--port", "8000"]

