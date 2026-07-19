from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
EXPECTED = [
    "Assets/PineMorphLab/ArtSource/Blender/pinemorph_asset_pack.blend",
    "Assets/PineMorphLab/ArtSource/Concept/pinemorph_asset_reference_v1.png",
    "Assets/PineMorphLab/Resources/Models/HumidityStage.fbx",
    "Assets/PineMorphLab/Resources/Models/PrecisionClamp.fbx",
    "Assets/PineMorphLab/Resources/Models/PineConeReference.fbx",
    "Assets/PineMorphLab/Resources/Models/BilayerCoupon.fbx",
    "Assets/PineMorphLab/Resources/Models/ScaleCrossSection.fbx",
    "Assets/PineMorphLab/Resources/Hud/hud_humidity_stage.png",
    "Assets/PineMorphLab/Resources/Hud/hud_precision_clamp.png",
    "Assets/PineMorphLab/Resources/Hud/hud_pine_cone.png",
    "Assets/PineMorphLab/Resources/Hud/hud_bilayer_coupon.png",
    "Assets/PineMorphLab/Resources/Hud/hud_scale_section.png",
]
missing = [path for path in EXPECTED if not (ROOT / path).is_file()]
empty = [path for path in EXPECTED if (ROOT / path).is_file() and (ROOT / path).stat().st_size == 0]
if missing or empty:
    raise SystemExit(f"PineMorph asset verification failed. missing={missing} empty={empty}")
print(f"PINEMORPH_ASSET_PACK_OK files={len(EXPECTED)}")
