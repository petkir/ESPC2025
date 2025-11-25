---
theme: default
layout: default
---

## Server‑Sent Events (SSE)

```
Content-Type: text/event-stream
data: <payload line 1>
data: <payload line 2>
event: <optional custom name>
id: <optional sequence>
retry: <ms>
```


### .NET 10 Highlights
- Native helpers for streaming responses
- Better cancellation & flush semantics
- Minimal API pattern fits SSE well

