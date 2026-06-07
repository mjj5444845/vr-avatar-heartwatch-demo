# Demo Script

## One-minute Walkthrough

1. Open the React dashboard.
2. Start mock heart-rate streaming, or sync from the SQLite API.
3. Show the avatar mood changing across calm, active, elevated, and high zones.
4. Open the SQLite API and show that samples are stored.
5. In Unity, run the Quest 3 scene and point out that the avatar mood uses `/api/latest`.
6. Explain that the Apple Watch path uses HealthKit on watchOS and WatchConnectivity through iPhone.

## Story

The avatar does not just wait for commands. It senses the user's physiological state and adapts the VR interaction style.

## Next Upgrade

Create the Xcode watchOS target from `sensor/apple-watch`, then connect it to the same SQLite API used by the React dashboard and Unity app.
