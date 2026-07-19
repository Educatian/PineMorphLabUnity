# PineMorph Lab

**An evidence-centered Unity simulation for learning humidity-responsive bio-inspired actuator design.**

**[Play the WebGL simulation](https://pinemorph-lab-unity.pages.dev)** | **[Browse the GitHub repository](https://github.com/Educatian/PineMorphLabUnity)**

PineMorph Lab asks learners to design a passive bilayer actuator inspired by pine-cone scales. Learners control active-layer thickness, layer stiffness ratio, and fiber orientation, then connect differential hygroscopic strain to laminate curvature, moisture response time, and structural stress.

The current revision is an English-only instructional MVP for a 10-15 minute mechanical-engineering or bio-inspired design activity. It is not a certified material-selection or facade-design tool.

The simulator is designed for a desktop or laptop browser with a mouse or trackpad. Phones and tablets receive a lightweight device notice instead of downloading the Unity player.

## Learning experience

During five Predict-Observe-Explain trials, learners:

1. rotate and zoom the model, then inspect clickable biological and engineered objects;
2. manipulate each design control in a required guided practice;
3. predict the limiting outcome before each test;
4. isolate fiber orientation, thickness, and stiffness effects;
5. follow a synchronized mechanics trace from humidity to design constraints;
6. compare opening angle, response time, and peak stress across trials;
7. revise an unsafe final design until all three constraints are met;
8. complete a Claim-Evidence-Reasoning check; and
9. export anonymous trial data as CSV.

## Instructional constraints

| Output | Target |
| --- | ---: |
| Opening angle | `45-75 deg` |
| Moisture response time, t95 | `<= 180 s` |
| Peak layer stress | `<= 3.5 MPa` |

These thresholds define the instructional optimization problem. They are starting values for learning design and require application-specific calibration before engineering use.

## Mechanics trace

```text
relative humidity
-> transformed hygroscopic strain
-> laminate force and moment equilibrium
-> curvature and moisture diffusion
-> angle, response time, and stress constraints
```

The 3D bilayer mesh, trace, metric dashboard, and cumulative graph all use the same `PineMorphResult` produced by the analytical model.

The graph is a normalized constraint map. Teal, amber, and coral encode opening angle, response time, and peak stress, while persistent horizontal rules show the instructional limits. Trial reruns replace the current record so a learner cannot accidentally duplicate evidence.

## Controls

| Input | Action |
| --- | --- |
| Active Layer Fraction | Changes laminate geometry and diffusion distance |
| Stiffness Ratio Ea/Ep | Redistributes force, moment, curvature, and stress |
| Fiber Angle | Transforms hygroscopic expansion along the hinge axis |
| Drag in 3D viewport | Rotate the model |
| Mouse wheel | Zoom |
| Click a layer or pine cone | Highlight the object and reveal its mechanical role |
| `R` | Reset the camera |

All sliders remain adjustable during every trial. Changing a design after selecting a prediction clears that prediction so the tested design and recorded prediction cannot become mismatched.

## Validation

- Unity `6000.4.9f1`
- `10/10` EditMode analytical physics tests
- automated PlayMode verification of required rotate, zoom, object-selection, and slider practice
- automated prediction, run, result, and navigation QA
- PlayMode verification that rerunning a trial replaces, rather than duplicates, its record
- runtime-generated 3D bilayer and English UGUI interface
- deterministic orbit verification with distinct before/after visual evidence
- anonymous JSONL/WebGL learning events
- WebGL-compatible CSV download bridge
- Chromium WebGL workflow QA at `1440 x 900`
- consistent phone and `1024 x 768` tablet guard verification

## Open the project

1. Add this directory in Unity Hub.
2. Open with Unity `6000.4.9f1`.
3. Load `Assets/PineMorphLab/Scenes/PineMorphLab.unity`.
4. Enter Play Mode.

Use `Tools > PineMorph Lab > Build Windows` or `Tools > PineMorph Lab > Build WebGL` for players.

## Documentation

- [Physics model](Documentation/PHYSICS_MODEL.md)
- [Evidence-centered design](GAME_CONCEPT.md)
- [Learning guide](LEARNING_GUIDE.md)
- [Design system](DESIGN.md)

## Engineering limitations

The present model assumes a slender, perfectly bonded, linear-elastic bilayer, uniform humidity through each layer at equilibrium, constant material properties, and first-mode through-thickness diffusion. Physical calibration should measure hygroexpansion, orthotropic stiffness, diffusivity, interface behavior, hysteresis, and cyclic fatigue for the intended material system.
