#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

if [[ ! -f .env ]]; then
  echo "Copy .env.example to .env and set RELAY_DB_SA_PASSWORD first." >&2
  exit 1
fi

if curl -s -o /dev/null http://localhost:5080; then
  echo "Port 5080 is already in use; stop the other API first." >&2
  exit 1
fi
if curl -s -o /dev/null http://localhost:4200; then
  echo "Port 4200 is already in use; stop the other web server first." >&2
  exit 1
fi

api_pid=""
web_pid=""
kill_tree() {
  local child
  for child in $(pgrep -P "$1" 2>/dev/null); do
    kill_tree "$child"
  done
  kill "$1" 2>/dev/null || true
}
stop_all() {
  trap '' INT TERM
  # dotnet run and npm start both launch the real server as a child, so stop the whole tree.
  for pid in $web_pid $api_pid; do
    kill_tree "$pid"
    wait "$pid" 2>/dev/null || true
  done
  echo "Database is still running. Stop it with: docker compose down (add -v to wipe the data)"
}
trap stop_all EXIT
trap 'exit 130' INT TERM

docker compose up -d --wait db

if [[ ! -f web/node_modules/.package-lock.json ]]; then
  (cd web && npm ci)
fi

dotnet run --project src/Relay.Api &
api_pid=$!

echo "Waiting for the API on http://localhost:5080 ..."
for _ in $(seq 1 120); do
  if ! kill -0 "$api_pid" 2>/dev/null; then
    echo "The API exited before it became ready." >&2
    exit 1
  fi
  if curl -fsS -o /dev/null http://localhost:5080/api/accounts 2>/dev/null; then
    break
  fi
  sleep 1
done
if ! curl -fsS -o /dev/null http://localhost:5080/api/accounts; then
  echo "The API did not answer on http://localhost:5080/api/accounts within 120 s." >&2
  exit 1
fi

echo "Open http://localhost:4200"
(cd web && npm start) &
web_pid=$!
wait "$web_pid"
