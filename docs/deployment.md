# Deployment

## Vercel

1. Import `mjj5444845/vr-avatar-heartwatch-demo` in Vercel.
2. Keep the default build settings from `vercel.json`.
3. Deploy.

The Vercel site is a static project README. It explains the architecture, setup flow, iPhone install flow, API contract, and test checklist. It does not fetch live Apple Watch data.

## GitHub Pages

1. Open the GitHub repo.
2. Go to **Settings > Pages**.
3. Set source to **GitHub Actions**.
4. Push to `main` or run the workflow manually.

The workflow builds `apps/web` and publishes `apps/web/dist`. This is the same static project README site.

## SQLite Hosting Note

Local SQLite works through `apps/api`. Vercel and GitHub Pages do not provide a durable writable SQLite file for production. For a hosted SQLite-compatible version, use Turso/libSQL or run `apps/api` on a small VM.

For this demo, the simplest reliable live setup is:

1. Run `npm run api:dev` on your Mac.
2. Run the iPhone app on the same Wi-Fi and set its API base URL to your Mac LAN URL.
3. Point Unity Quest 3 and the iPhone app at `http://YOUR_MAC_IP:8787`.
4. Open HeartWatch on Apple Watch, tap **Start**, and keep the iPhone app open so live samples post into SQLite.
5. Use Vercel/GitHub Pages only for static project documentation.
