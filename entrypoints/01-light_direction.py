import numpy as np
import sys
sys.path.insert(0, r"C:\Users\rafae\2026-cg03-pbr-243360_245609\src")
import urenderer

# Sombreia sem utilizar informações da superfície

# Você precisa escrever vertex.vs para passar as posições, normais e UVs para o fragment
# e escrever urenderer/renderer/opengl/shader_library/light.glsl e 01-fragment.fs para realizar o sombreamento.
import os
print("CWD:", os.getcwd())


with open("01-fragment.fs", "r") as f:
    print(repr(f.read()[:150]))
if __name__ == "__main__":
    urenderer.utils.clear_workdir("01-light_direction")
    renderer = urenderer.renderer.OpenGLRenderer(1920, 1080)
    renderer.background_color = np.array([0, 0, 0, 1], np.float32)
    runtime = urenderer.application.Runtime(
        renderer, name="01-light_direction")

    shader = urenderer.renderer.Shader("vertex.vs", "01-fragment.fs")
    material = urenderer.renderer.opengl.Material(shader)

    sphere = urenderer.node.Node()

    sphere.translation = np.array([0, 0, -5], np.float64)
    sphere.render_data["mesh"] = urenderer.geometry.mesh.get_mesh_sphere()
    sphere.render_data["material"] = material

    sphere2 = sphere.clone()
    sphere2.translation = np.array([-2.5, 0, -5], np.float64)

    sphere3 = sphere.clone()
    sphere3.translation = np.array([2.5, 0, -5], np.float64)

    runtime.scene.add_child(sphere)
    runtime.scene.add_child(sphere2)
    runtime.scene.add_child(sphere3)

    light = urenderer.node.Light(urenderer.node.LightType.DIRECTIONAL)
    light.rotation = np.array([45, 45, 45], np.float64)
    runtime.scene.add_child(light)

    light2 = urenderer.node.Light(urenderer.node.LightType.POINT)
    light2.translation = np.array([4, -1, -4], np.float64)
    light2.light_color = np.array([0.5, 0.0, 0.0], np.float32)
    runtime.scene.add_child(light2)

    light3 = urenderer.node.Light(urenderer.node.LightType.POINT)
    light3.translation = np.array([-4, -1, -4], np.float64)
    light3.light_color = np.array([0.0, 0.0, 0.5], np.float32)
    runtime.scene.add_child(light3)

    light4 = urenderer.node.Light(urenderer.node.LightType.POINT)
    light4.translation = np.array([-1.25, 1, -4], np.float64)
    light4.light_color = np.array([0.5, 0.0, 0.5], np.float32)
    light4.light_intensity = 5.0
    runtime.scene.add_child(light4)

    light4 = urenderer.node.Light(urenderer.node.LightType.POINT)
    light4.translation = np.array([0, 1, -4], np.float64)
    light4.light_color = np.array([0.0, 1.0, 0.0], np.float32)
    light4.light_intensity = 0.05
    light4.light_reference_distance = 10.0
    runtime.scene.add_child(light4)

    runtime.loop(capture=[1])
