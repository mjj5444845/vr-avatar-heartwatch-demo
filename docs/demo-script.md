# Demo Script

## One-minute Walkthrough

1. Start the SQLite API on the Mac.
2. Open the iPhone app and confirm it can sync from the API.
3. Start the Apple Watch heart-rate stream and show the iPhone chart updating.
4. Open the SQLite API and show that samples are stored.
5. In Unity, run the Quest 3 scene and point out that the heart-rate panel uses `/api/latest`.
6. Advance a scripted dialogue and show the conversation rows in the iPhone app.
7. Explain that the Apple Watch path uses HealthKit on watchOS and WatchConnectivity through iPhone.

## Story

The avatar does not just wait for commands. It senses the user's physiological state and adapts the VR interaction style.

## Next Upgrade

Package the Xcode iPhone + watchOS targets from `sensor/apple-watch`, then connect them to the same SQLite API used by the Unity app.
