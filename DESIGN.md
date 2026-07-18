# PineMorph Lab Design System

## Identity

PineMorph Lab is a compact engineering instrument inspired by botanical material systems. Dark forest ink communicates analytical structure, teal identifies active mechanics, amber marks time-related trade-offs, coral marks structural risk, and restrained brown identifies the passive biological layer.

## Layout

The 1600 x 900 reference canvas uses a fixed header, left design rail, central unframed 3D viewport, right evidence dashboard, and bottom result and trial record band. Controls and data retain stable dimensions during animation.

### Layout tokens

| Token | Value | Use |
|---|---:|---|
| `LeftX` / `LeftWidth` | 20 / 350 | Design controls rail |
| `CenterX` / `CenterWidth` | 388 / 750 | Learning stage, viewport, results |
| `RightX` / `RightWidth` | 1154 / 426 | Evidence dashboard |
| `WorkspaceTop` | 80 | Shared panel origin |
| `ResultsTop` | 632 | Stable bottom evidence band |

The WebGL wrapper supports the full simulator at 1100 px and wider. Narrower windows and touch-device user agents receive one consistent desktop-required guard before Unity loads.

## Components

- **Parameter slider:** label, physical interpretation, live value, teal fill, white handle.
- **Prediction button:** compact outcome command with selected and cleared states.
- **Metric instrument:** dark surface, colored top rule, value, unit, and constraint status.
- **Mechanics trace:** five persistent causal stages with one active stage.
- **Guided learning overlay:** learner-paced Concept-Do-Watch For scaffold with Back, Next, and Skip.

## Accessibility

All learning content is English text, no instruction depends on color alone, tutorial progress is persistent, animation never auto-advances the lesson, and the camera ignores pointer input over UI. Narrow mobile screens receive a desktop-required WebGL guard.
