# Deployment

## Vercel

1. Import `mjj5444845/vr-avatar-heartwatch-demo` in Vercel.
2. Keep the default build settings from `vercel.json`.
3. Add `VITE_API_BASE_URL` only if you have a reachable API endpoint.
4. Deploy.

The React dashboard can run without an API in mock mode.

If you want the hosted Vercel dashboard to show live Apple Watch data, `VITE_API_BASE_URL` must point to a public HTTPS API. A local Mac URL such as `http://192.168.1.20:8787` only works for devices on the same network and is not reachable from Vercel users outside that network.

## GitHub Pages

1. Open the GitHub repo.
2. Go to **Settings > Pages**.
3. Set source to **GitHub Actions**.
4. Push to `main` or run the workflow manually.

The workflow builds `apps/web` and publishes `apps/web/dist`.

## SQLite Hosting Note

Local SQLite works through `apps/api`. Vercel and GitHub Pages do not provide a durable writable SQLite file for production. For a hosted SQLite-compatible version, use Turso/libSQL or run `apps/api` on a small VM.

For this demo, the simplest reliable live setup is:

1. Run `npm run api:dev` on your Mac.
2. Run the iPhone/Apple Watch bridge on the same Wi-Fi.
3. Point Unity Quest 3 and the local React dashboard at `http://YOUR_MAC_IP:8787`.
4. Use Vercel/GitHub Pages for mock mode or for a dashboard connected to a separately hosted API.
