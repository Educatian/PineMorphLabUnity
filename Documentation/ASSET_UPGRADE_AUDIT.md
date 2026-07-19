# PineMorph Blender Asset Upgrade Audit

## Upgrade objective

Raise PineMorph to the same immersive asset standard as GeckoGrip while keeping the live bilayer mesh, analytical laminate mechanics, moisture diffusion model, and evidence workflow intact.

## Gap-to-asset map

| Previous limitation | Blender replacement | Learning purpose |
| --- | --- | --- |
| Flat ribbon without bonded-layer context | `BilayerCoupon.fbx` and HUD render with active, passive, bond, and fiber layers | Makes thickness fraction and fiber orientation visually interpretable |
| Cylinder platform | `HumidityStage.fbx` with machined plinth, specimen window, sensor, vapor manifold, guard, and fasteners | Connects humidity input to a plausible laboratory apparatus |
| Cube clamps | `PrecisionClamp.fbx` with jaw, soft insert, screw, and knob | Shows boundary conditions and load transfer |
| Sphere pile pine cone | `PineConeReference.fbx` with custom overlapping directed scales | Provides a recognizable biological reference with readable scale orientation |
| No tissue-scale specimen | `ScaleCrossSection.fbx` with active/passive tissues, cellulose bundles, and pores | Connects anisotropy and moisture transport to the biological structure |
| Text-only design controls | Five transparent HUD renders in `Resources/Hud` | Connects the three sliders to physical geometry and test hardware |

## Production files

- Editable source: `Assets/PineMorphLab/ArtSource/Blender/pinemorph_asset_pack.blend`
- Concept sheet: `Assets/PineMorphLab/ArtSource/Concept/pinemorph_asset_reference_v1.png`
- Runtime FBX assets: `Assets/PineMorphLab/Resources/Models/`
- Runtime HUD renders: `Assets/PineMorphLab/Resources/Hud/`
- Blender geometry module: `Tools/pinemorph_blender_geometry.py`
- Rebuild driver: `Tools/build_pinemorph_assets.py`

## Integration contract

`PineMorphRibbon` remains the live deforming mesh driven by `PineMorphModel`. The Blender humidity stage, clamps, pine cone, and tissue section supply context and selectable reference objects. The result dashboard, trace, graph, CSV export, and competency evidence continue to consume the same analytical `PineMorphResult`.

## Current validation

- Unity compilation and WebGL player build: pass
- Desktop 1600 x 900 rest, rotate/zoom, and completed-trial captures: pass
- Camera screenshot hash before/after orbit and zoom: changed as expected
- Active-layer object selection: pass
- Three slider changes, prediction, two-stage test execution, graph update, and enabled CSV control: pass
- Browser console warnings/errors during the tested flow: none

## Remaining calibration boundary

The upgraded assets improve explanatory realism but do not convert the instructional model into a material-certification tool. Engineering use still requires measured orthotropic stiffness, hygroexpansion, diffusivity, interface strength, hysteresis, and fatigue data.
