# Common Bio-Design Learning Event Schema

PineMorph Lab emits versioned, anonymous JSON events using `bio-design-learning-event/1.0`. The other two labs use the same field names and semantics so evidence can be combined without app-specific remapping.

| Field | Meaning |
| --- | --- |
| `appId`, `sessionId`, `timestampUtc`, `eventName` | Event identity and ordering |
| `opportunityIndex` | One-based assessment opportunity, or `0` outside a trial |
| `opportunitiesCompleted`, `opportunitiesAvailable` | Raw opportunity counts |
| `normalizedOpportunityProgress` | `completed / available`, clamped to `0-1` |
| `inputName`, `inputValue` | Changed design variable and numeric value |
| `prediction`, `confidence` | Pre-result prediction and confidence (`0` when not reported) |
| `result`, `constraintFlags` | Observed outcome and machine-readable constraint states |
| `revisionAttempt` | Current redesign attempt count |
| `isFinalDesign`, `finalDesign` | Final-design marker and serialized design state |
| `competencyScore`, `detail` | Session score and event-specific context |

FinGrip and PineMorph expose five trial opportunities; GeckoGrip exposes four. Cross-app summaries should compare `normalizedOpportunityProgress` and normalized component scores, not raw event counts.

WebGL dispatches each JSON object as a `pinemorph-learning-event` browser event and as an anonymous `postMessage` payload. Native players append the same objects to the app's JSONL evidence file.
