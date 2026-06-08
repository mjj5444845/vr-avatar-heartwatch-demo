#!/bin/zsh
set -e

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT_DIR"

PORT="${PORT:-8787}"
SQLITE_PATH="${SQLITE_PATH:-$ROOT_DIR/apps/api/data/demo.sqlite}"
MAC_IP="$(ipconfig getifaddr en0 2>/dev/null || ipconfig getifaddr en1 2>/dev/null || echo "YOUR_MAC_IP")"
LOG_DIR="$ROOT_DIR/logs"
RUN_STAMP="$(date +%Y%m%d-%H%M%S)"
RUN_LOG="$LOG_DIR/demo-run-$RUN_STAMP.log"
API_LOG="$LOG_DIR/api-output-$RUN_STAMP.log"

mkdir -p "$LOG_DIR"
exec > >(tee -a "$RUN_LOG") 2>&1

echo ""
echo "VR Avatar HeartWatch Demo"
echo "============================================================"
echo "Run log: $RUN_LOG"
echo "API log: $API_LOG"
echo "SQLite path: $SQLITE_PATH"
echo "1. SQLite API: http://$MAC_IP:$PORT"
echo "2. iPhone app API URL: http://$MAC_IP:$PORT"
echo "3. Apple Watch: open HeartWatch and tap Start"
echo "4. Quest 3: press right-hand B to start the VR demo"
echo "============================================================"
echo ""

if [ ! -d "$ROOT_DIR/node_modules" ]; then
  echo "Installing Node dependencies..."
  npm install
fi

mkdir -p "$(dirname "$SQLITE_PATH")"

echo "Starting SQLite API..."
HOST=0.0.0.0 PORT="$PORT" SQLITE_PATH="$SQLITE_PATH" npm run api:start >> "$API_LOG" 2>&1 &
API_PID=$!

sleep 2
echo ""
echo "API health check:"
curl -s "http://127.0.0.1:$PORT/api/demo/status" || true
echo ""
echo "iPhone quick test:"
echo "1. Open Safari on iPhone."
echo "2. Visit http://$MAC_IP:$PORT/api/health"
echo "3. It should show JSON with ok=true."
echo ""

MAC_APP="$ROOT_DIR/release/macOS/VRAvatarHeartWatch.app"
UNITY_PROJECT="$ROOT_DIR/unity/VRAvatarHeartWatch"
UNITY_EDITOR="/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app"

if [ -d "$MAC_APP" ]; then
  echo "Opening built macOS demo app..."
  open "$MAC_APP"
elif [ -d "$UNITY_EDITOR" ]; then
  echo "No built macOS app found. Opening Unity project..."
  open -a "$UNITY_EDITOR" --args -projectPath "$UNITY_PROJECT"
else
  echo "Unity build not found at $MAC_APP and Unity Editor was not found."
  echo "Open this project manually in Unity: $UNITY_PROJECT"
fi

echo ""
echo "Keep this window open while presenting. Press Ctrl+C to stop the API."
wait "$API_PID"
