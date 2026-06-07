# Database Schema

The demo uses SQLite through `apps/api`. The canonical schema lives in `database/schema.sql`.

## heart_rate_samples

| Field | Type | Notes |
| --- | --- | --- |
| id | string | UUID |
| session_id | string | Add when multi-session support is needed |
| source | string | `apple_watch_mock` or `apple_watch` |
| heart_rate | number | BPM |
| timestamp | datetime | ISO string |

## Local Database

By default the API writes to:

```text
apps/api/data/demo.sqlite
```

Override with:

```bash
SQLITE_PATH=/absolute/path/demo.sqlite npm run api:dev
```

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

## chat_messages

| Field | Type | Notes |
| --- | --- | --- |
| id | string | UUID |
| role | string | `user`, `avatar`, or `system` |
| text | string | Dialogue text shown in VR and the iPhone app |
| message_type | string | `user_speech`, `avatar_reply`, `sensor_prompt`, or `system` |
| conversation_initiator | string | `user` or `avatar` |
| heart_rate | number | BPM at the moment of the record, when available |
| zone | string | calm, active, elevated, high, or unknown |
| timestamp | datetime | ISO string |

The Unity scripted dialogue writes to `/api/chat/records`; Apple Watch samples write to `/api/samples`.
