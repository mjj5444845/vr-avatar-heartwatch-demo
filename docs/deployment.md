# Deployment

## Vercel

1. Import `mjj5444845/vr-avatar-heartwatch-demo` in Vercel.
2. Keep the default build settings from `vercel.json`.
3. Add `VITE_API_BASE_URL` only if you have a reachable API endpoint.
4. Deploy.

The React dashboard can run without an API in mock mode.

## GitHub Pages

1. Open the GitHub repo.
2. Go to **Settings > Pages**.
3. Set source to **GitHub Actions**.
4. Push to `main` or run the workflow manually.

The workflow builds `apps/web` and publishes `apps/web/dist`.

## SQLite Hosting Note

Local SQLite works through `apps/api`. Vercel and GitHub Pages do not provide a durable writable SQLite file for production. For a hosted SQLite-compatible version, use Turso/libSQL or run `apps/api` on a small VM.

