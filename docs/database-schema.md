# Database Schema

Start with `localStorage` through `src/database.js`. Replace that adapter later without changing the rest of the app.

## heart_rate_samples

| Field | Type | Notes |
| --- | --- | --- |
| id | string | UUID |
| session_id | string | Add when multi-session support is needed |
| source | string | `apple_watch_mock` or `apple_watch` |
| heart_rate | number | BPM |
| timestamp | datetime | ISO string |

## avatar_messages

| Field | Type | Notes |
| --- | --- | --- |
| id | string | UUID |
| session_id | string | Add when multi-session support is needed |
| zone | string | calm, active, elevated, high |
| tone | string | Avatar behavior mode |
| text | string | Message shown to the user |
| timestamp | datetime | ISO string |

## vr_events

| Field | Type | Notes |
| --- | --- | --- |
| id | string | UUID |
| session_id | string | Add when multi-session support is needed |
| type | string | Interaction event name |
| text | string | Human-readable event summary |
| timestamp | datetime | ISO string |

