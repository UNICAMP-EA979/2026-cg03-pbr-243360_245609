import numpy as np

#import sys
#sys.path.insert(0, r"C:\Users\rafae\2026-cg03-pbr-243360_245609\src")
import urenderer

# Você deve acrescentar luz especular ao modelo de sombreamento
#
# Altere os arquivos .../shader_library/specular.glsl,
# e 03-fragment.fs para realizar o sombreamento.

urenderer.utils.clear_workdir("03-specular")
renderer = urenderer.renderer.OpenGLRenderer(1920, 1080)
renderer.background_color = np.array([0, 0, 0, 1], np.float32)
runtime = urenderer.application.Runtime(
    renderer, name="03-specular")

shader = urenderer.renderer.Shader("vertex.vs", "03-fragment.fs")
material = urenderer.renderer.opengl.Material(shader)

sphere = urenderer.node.Node()

sphere.translation = np.array([0, 0, -5], np.float64)
sphere.render_data["mesh"] = urenderer.geometry.mesh.get_mesh_sphere()
sphere.render_data["material"] = material

runtime.scene.add_child(sphere)

light = urenderer.node.Light(urenderer.node.LightType.DIRECTIONAL)
light.rotation = np.array([45, 45, 45], np.float64)
light.light_intensity = 3.0
runtime.scene.add_child(light)

runtime.loop(capture=[1])
