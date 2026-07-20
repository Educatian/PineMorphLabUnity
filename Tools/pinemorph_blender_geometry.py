from __future__ import annotations

import math

import bpy
from mathutils import Vector


def collection(name: str):
    target = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(target)
    return target


def finish(obj, target, mat, bevel: float = 0.0):
    for source in tuple(obj.users_collection):
        source.objects.unlink(obj)
    target.objects.link(obj)
    if mat is not None:
        obj.data.materials.append(mat)
    if bevel > 0.0:
        modifier = obj.modifiers.new("Manufactured edge", "BEVEL")
        modifier.width = bevel
        modifier.segments = 3
    return obj


def cube(name: str, target, location, scale, mat, bevel: float = 0.04):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = (scale[0] * 0.5, scale[1] * 0.5, scale[2] * 0.5)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish(obj, target, mat, bevel)


def sphere(name: str, target, location, scale, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=18, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    finish(obj, target, mat, 0.018)
    bpy.ops.object.shade_smooth()
    return obj


def cylinder(name: str, target, location, radius: float, depth: float, mat, rotation=(0.0, 0.0, 0.0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=radius, depth=depth, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    finish(obj, target, mat, min(radius * 0.18, 0.035))
    bpy.ops.object.shade_smooth()
    return obj


def build_stage(mats):
    target = collection("HumidityStage")
    cube("MachinedPlinth", target, (0.0, 0.0, 0.08), (4.3, 3.0, 0.16), mats["steel"], 0.10)
    cube("RaisedDeck", target, (0.0, 0.0, 0.28), (3.55, 2.25, 0.22), mats["dark_steel"], 0.07)
    cube("SpecimenWindow", target, (0.0, 0.05, 0.41), (2.25, 1.18, 0.035), mats["glass"], 0.02)
    cylinder("HumiditySensor", target, (-1.45, 0.78, 0.95), 0.16, 1.10, mats["sensor"])
    cylinder("SensorGuard", target, (-1.45, 0.78, 1.52), 0.22, 0.30, mats["steel"])
    cube("VaporManifold", target, (1.42, 0.72, 0.62), (0.55, 0.42, 0.70), mats["housing"], 0.07)
    for index in range(4):
        cylinder("VaporPort", target, (1.22 + index * 0.14, 0.38, 0.92), 0.045, 0.24, mats["steel"],
                 (math.pi * 0.5, 0.0, 0.0))
    for x in (-1.85, 1.85):
        for y in (-1.18, 1.18):
            cylinder("DeckFastener", target, (x, y, 0.20), 0.07, 0.08, mats["dark_steel"])
    return target


def build_clamp(mats):
    target = collection("PrecisionClamp")
    cube("ClampBase", target, (0.0, 0.0, 0.12), (0.82, 0.72, 0.24), mats["dark_steel"], 0.07)
    cube("ClampJaw", target, (0.0, 0.08, 0.48), (0.72, 0.24, 0.52), mats["housing"], 0.06)
    cube("SoftJawInsert", target, (0.0, -0.07, 0.50), (0.55, 0.08, 0.36), mats["rubber"], 0.025)
    cylinder("LeadScrew", target, (0.0, 0.18, 0.85), 0.075, 0.55, mats["steel"])
    cylinder("Knob", target, (0.0, 0.18, 1.16), 0.18, 0.16, mats["dark_steel"])
    for x in (-0.28, 0.28):
        cylinder("ClampFastener", target, (x, 0.0, 0.26), 0.055, 0.07, mats["steel"])
    return target


def scale_mesh(name: str, target, mat):
    longitudinal = 7
    transverse = 5
    vertices = []
    faces = []
    for row in range(longitudinal):
        u = row / (longitudinal - 1)
        y = -0.34 + u * 1.08
        half_width = 0.29 * (1.0 - u ** 1.55) + 0.025
        crown = 0.055 * math.sin(u * math.pi) + 0.16 * u * u
        for col in range(transverse):
            v = -1.0 + 2.0 * col / (transverse - 1)
            x = half_width * v
            edge_curl = 0.035 * v * v * (0.4 + u)
            vertices.append((x, y, crown + edge_curl))
    for row in range(longitudinal - 1):
        for col in range(transverse - 1):
            a = row * transverse + col
            b = a + 1
            c = a + transverse + 1
            d = a + transverse
            faces.append((a, b, c, d))
    mesh = bpy.data.meshes.new(f"{name}Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    target.objects.link(obj)
    obj.data.materials.append(mat)
    solidify = obj.modifiers.new("Scale thickness", "SOLIDIFY")
    solidify.thickness = 0.045
    solidify.offset = -0.35
    bevel = obj.modifiers.new("Weathered scale edge", "BEVEL")
    bevel.width = 0.018
    bevel.segments = 2
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.shade_smooth()
    obj.select_set(False)
    return obj


def build_pine_cone(mats):
    target = collection("PineConeReference")
    cylinder("ConeCore", target, (0.0, 0.0, 0.92), 0.34, 1.82, mats["wood_dark"])
    rows = 12
    for row in range(rows):
        height_ratio = row / (rows - 1)
        count = 9 + row
        radius = 0.25 + math.sin((row + 1.0) / (rows + 1.0) * math.pi) * 0.49
        z = 0.12 + row * 0.15
        scale_size = 0.70 + 0.15 * math.sin(height_ratio * math.pi)
        material_key = ("wood_light" if (row + 1) % 3 == 0
                        else "wood_mid" if row % 3 == 0
                        else "wood")
        for index in range(count):
            angle = (index + row * 0.48) * math.tau / count
            item = scale_mesh(f"Scale_{row}_{index}", target, mats[material_key])
            item.location = (math.cos(angle) * radius, math.sin(angle) * radius, z)
            flare = 0.54 - height_ratio * 0.22
            item.rotation_euler = (flare, 0.0, angle - math.pi * 0.5)
            item.scale = (scale_size, scale_size * 0.90, scale_size)
    return target


def build_coupon(mats):
    target = collection("BilayerCoupon")
    cube("PassiveLayer", target, (0.0, 0.0, 0.10), (3.2, 1.25, 0.20), mats["passive"], 0.05)
    cube("ActiveLayer", target, (0.0, 0.0, 0.29), (3.2, 1.25, 0.18), mats["active"], 0.05)
    for index in range(13):
        y = -0.52 + index * 0.087
        cube(f"Fiber_{index}", target, (0.0, y, 0.40), (2.92, 0.025, 0.025), mats["fiber"], 0.008)
    cube("BondLine", target, (0.0, 0.0, 0.205), (3.08, 1.18, 0.025), mats["bond"], 0.01)
    return target


def build_scale_section(mats):
    target = collection("ScaleCrossSection")
    cube("PassiveTissue", target, (0.0, 0.0, 0.20), (2.6, 1.0, 0.40), mats["passive"], 0.08)
    cube("ActiveTissue", target, (0.0, 0.0, 0.58), (2.6, 1.0, 0.36), mats["active"], 0.08)
    for index in range(12):
        x = -1.12 + index * 0.205
        cylinder("CelluloseBundle", target, (x, 0.0, 0.62), 0.035, 0.88, mats["fiber"],
                 (math.pi * 0.5, 0.0, 0.0))
    for x in (-1.0, -0.55, 0.0, 0.55, 1.0):
        sphere("Pore", target, (x, -0.48, 0.22), (0.10, 0.035, 0.10), mats["pore"])
    return target
