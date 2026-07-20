# /// script
# requires-python = ">=3.11"
# dependencies = []
# ///
# blender --background --python Tools/build_pinemorph_assets.py

from __future__ import annotations

from pathlib import Path
import sys
from typing import Final

import bpy
from mathutils import Vector

sys.path.insert(0, str(Path(__file__).resolve().parent))

from pinemorph_blender_geometry import (
    build_clamp,
    build_coupon,
    build_pine_cone,
    build_scale_section,
    build_stage,
)

ROOT: Final = Path(__file__).resolve().parents[1]
MODEL_ROOT: Final = ROOT / "Assets/PineMorphLab/Resources/Models"
HUD_ROOT: Final = ROOT / "Assets/PineMorphLab/Resources/Hud"
BLEND_PATH: Final = ROOT / "Assets/PineMorphLab/ArtSource/Blender/pinemorph_asset_pack.blend"
REFERENCE_PATH: Final = ROOT / "Assets/PineMorphLab/ArtSource/Concept/pinemorph_asset_reference_v1.png"


def reset_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for item in tuple(bpy.data.collections):
        bpy.data.collections.remove(item)


def material(name: str, color, metallic: float, roughness: float):
    item = bpy.data.materials.new(name)
    item.diffuse_color = color
    item.use_nodes = True
    shader = item.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = color
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = roughness
    return item


def configure_render():
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE_NEXT"
    scene.render.resolution_x = 256
    scene.render.resolution_y = 256
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGBA"
    scene.render.film_transparent = True
    world = bpy.data.worlds.new("NeutralWorld")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.03, 0.04, 0.04, 1.0)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.42
    scene.world = world
    bpy.ops.object.light_add(type="AREA", location=(4.0, -5.0, 7.0))
    bpy.context.object.data.energy = 1050.0
    bpy.context.object.data.size = 5.0
    bpy.ops.object.light_add(type="AREA", location=(-4.0, 2.0, 4.0))
    bpy.context.object.data.energy = 580.0
    bpy.context.object.data.color = (0.44, 0.72, 0.58)
    bpy.context.object.data.size = 4.0
    bpy.ops.object.camera_add()
    camera = bpy.context.object
    camera.data.type = "ORTHO"
    scene.camera = camera
    return camera


def render_icon(target, camera, filename: str) -> None:
    for obj in bpy.context.scene.objects:
        visible = obj.name in target.objects
        obj.hide_render = obj.name != camera.name and obj.type != "LIGHT" and not visible
    points = [obj.matrix_world @ Vector(corner) for obj in target.objects for corner in obj.bound_box]
    low = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
    high = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
    center = (low + high) * 0.5
    size = max(high.x - low.x, high.y - low.y, high.z - low.z)
    camera.location = center + Vector((1.30, -1.65, 1.16)) * size
    camera.rotation_euler = (center - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.ortho_scale = size * 1.18
    bpy.context.scene.render.filepath = str(HUD_ROOT / filename)
    bpy.ops.render.render(write_still=True)


def export_collection(target, filename: str) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    for obj in target.objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = next(iter(target.objects))
    bpy.ops.export_scene.fbx(
        filepath=str(MODEL_ROOT / filename), use_selection=True,
        axis_forward="-Z", axis_up="Y", apply_scale_options="FBX_SCALE_ALL",
        add_leaf_bones=False, bake_anim=False, mesh_smooth_type="FACE",
    )


def main() -> None:
    MODEL_ROOT.mkdir(parents=True, exist_ok=True)
    HUD_ROOT.mkdir(parents=True, exist_ok=True)
    BLEND_PATH.parent.mkdir(parents=True, exist_ok=True)
    reset_scene()
    bpy.data.images.load(str(REFERENCE_PATH), check_existing=True).name = "ArtDirection_PineMorph"
    mats = {
        "steel": material("BrushedSteel", (0.46, 0.49, 0.50, 1.0), 0.82, 0.24),
        "dark_steel": material("DarkSteel", (0.035, 0.045, 0.05, 1.0), 0.72, 0.30),
        "housing": material("ClampHousing", (0.025, 0.055, 0.055, 1.0), 0.42, 0.30),
        "rubber": material("SoftJaw", (0.025, 0.11, 0.095, 1.0), 0.0, 0.76),
        "glass": material("SpecimenWindow", (0.12, 0.42, 0.38, 1.0), 0.25, 0.18),
        "sensor": material("HumiditySensor", (0.70, 0.72, 0.69, 1.0), 0.68, 0.28),
        "wood": material("PineScale", (0.30, 0.125, 0.045, 1.0), 0.0, 0.70),
        "wood_mid": material("PineScaleMid", (0.40, 0.19, 0.075, 1.0), 0.0, 0.66),
        "wood_light": material("PineScaleLight", (0.52, 0.29, 0.13, 1.0), 0.0, 0.62),
        "wood_dark": material("PineCore", (0.12, 0.045, 0.018, 1.0), 0.0, 0.88),
        "active": material("ActiveTissue", (0.22, 0.42, 0.16, 1.0), 0.0, 0.78),
        "passive": material("PassiveTissue", (0.38, 0.17, 0.065, 1.0), 0.0, 0.84),
        "fiber": material("CelluloseFiber", (0.64, 0.72, 0.42, 1.0), 0.0, 0.52),
        "bond": material("BondLine", (0.72, 0.64, 0.32, 1.0), 0.0, 0.38),
        "pore": material("TissuePore", (0.10, 0.055, 0.025, 1.0), 0.0, 0.92),
    }
    assets = (
        (build_stage(mats), "HumidityStage.fbx", "hud_humidity_stage.png"),
        (build_clamp(mats), "PrecisionClamp.fbx", "hud_precision_clamp.png"),
        (build_pine_cone(mats), "PineConeReference.fbx", "hud_pine_cone.png"),
        (build_coupon(mats), "BilayerCoupon.fbx", "hud_bilayer_coupon.png"),
        (build_scale_section(mats), "ScaleCrossSection.fbx", "hud_scale_section.png"),
    )
    camera = configure_render()
    for target, model_name, hud_name in assets:
        export_collection(target, model_name)
        render_icon(target, camera, hud_name)
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))
    print("PINEMORPH_BLENDER_ASSETS_OK models=5 hud=5")


if __name__ == "__main__":
    main()
