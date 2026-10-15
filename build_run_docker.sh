#!/bin/bash
# Build and (re)start the API and UI containers for the local test environment.
#   Storefront: http://<host-ip>:8080/     API: http://<host-ip>:8003/api/...
#
# Needs DB_PASSWORD, STRIPE_SECRET_KEY, BREVO_API_KEY and JWT_SIGNING_KEY, either exported
# or in a .env file next to this script (docker compose reads it automatically).
set -euo pipefail
cd "$(dirname "$0")"

# Generate a JWT signing key once. Regenerating on every run would invalidate issued tokens
# and pile up duplicate keys in .env.
if ! grep -q '^JWT_SIGNING_KEY=..*' .env 2>/dev/null; then
  sed -i '/^JWT_SIGNING_KEY=$/d' .env 2>/dev/null || true
  echo "JWT_SIGNING_KEY=$(openssl rand -base64 48)" >> .env
fi

# Both compose files join this network; the UI's nginx reaches the API through it.
docker network inspect proxy-net >/dev/null 2>&1 || docker network create proxy-net

# API first: the UI proxies /api to it. set -e stops here if it fails (e.g. a missing secret).
docker compose up -d --build --force-recreate
docker compose -f ui/docker-compose.yml up -d --build --force-recreate
