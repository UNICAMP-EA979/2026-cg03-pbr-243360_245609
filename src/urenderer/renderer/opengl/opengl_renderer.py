import ctypes
from concurrent.futures import ProcessPoolExecutor
from typing import TYPE_CHECKING, Any, TypeAlias, cast

import cv2 as cv
import glfw
import numpy as np
from OpenGL import GL

from urenderer.geometry.mesh import Mesh
from urenderer.node import Camera, Light, Node
from urenderer.renderer.renderer import Renderer
from urenderer.utils import get_filename_unique

from .material import Material

if TYPE_CHECKING:
    GLFWWindow = Any
else:
    GLFWWindow = ctypes.POINTER(glfw._GLFWwindow)


def save_frame(path: str, frame: np.ndarray) -> None:
    '''
    Save a frame

    Args:
        path (str): path to save
        frame (np.ndarray): frame
    '''
    cv.imwrite(path, frame)


class OpenGLRenderer(Renderer):
    '''
    Renderer using OpenGL
    '''

    def __init__(self, screen_width: int, screen_height: int) -> None:
        '''
        OpenGLRenderer initializer.

        Args:
            screen_width (int): screen width
            screen_height (int): screen height
            show (bool, optional): if should show the rendered frame. Defaults to True.
        '''
        super().__init__(screen_width, screen_height)
        self._executor = ProcessPoolExecutor(max_workers=1)

        self._executor = ProcessPoolExecutor(max_workers=1)

        # Inicializa o GLFW, core profile e OpenGL 3.3
        if not glfw.init():
            raise RuntimeError("Failed to initialize GLFW")

        glfw.window_hint(glfw.CONTEXT_VERSION_MAJOR, 3)
        glfw.window_hint(glfw.CONTEXT_VERSION_MINOR, 3)
        glfw.window_hint(glfw.OPENGL_PROFILE, glfw.OPENGL_CORE_PROFILE)
        glfw.window_hint(glfw.OPENGL_FORWARD_COMPAT, GL.GL_TRUE)  # macOS compat

        # Cria a janela, associando ela ao contexto
        # e configurando o tamanho dela no OpenGL
        window = glfw.create_window(screen_width, screen_height,
                                    "urenderer", None, None)
        if not window:
            glfw.terminate()
            raise RuntimeError("Failed to create GLFW window")

        glfw.make_context_current(window)
        GL.glViewport(0, 0, screen_width, screen_height)

        glfw.set_framebuffer_size_callback(
            window, self._framebuffer_size_callback)

        GL.glEnable(GL.GL_DEPTH_TEST)

        self._window = cast(GLFWWindow, window)
        self.background_color = np.array([1.0, 0.0, 1.0, 1.0])

        self.ambient_color = np.array([0.0, 0.0, 0.0], dtype=np.float32)

        GL.glDisable(GL.GL_DITHER)

        # Habilita a conversão automática de RGB linear para sRGB (ajuste de gamma)
        # O OpenGL aplica a curva de gamma (≈ x^(1/2.2)) em cada fragmento antes
        # de gravar no framebuffer, garantindo que a saída seja perceptualmente correta.
        GL.glEnable(GL.GL_FRAMEBUFFER_SRGB)

    def _framebuffer_size_callback(self, window: GLFWWindow,
                                   width: int, height: int):
        '''
        Callback for a change in the framebuffer size

        Args:
            window (GLFWWindow): window with size change
            width (int): new width
            height (int): new heigth
        '''
        GL.glViewport(0, 0, width, height)

    def start(self, camera: Camera, view_matrix: np.ndarray, name: str) -> None:
        '''
        Start the frame rendering

        Args:
            camera (Camera): current camera.
            view_matrix (np.ndarray): camera view matrix.
            name (str): name of the application
        '''
        super().start(camera, view_matrix, name)
        self._view_matrix = view_matrix
        self._projection_matrix = camera.projection_matrix
        self._name = name
        self._lights: list[dict[str, Light | np.ndarray]] = []

        glfw.set_window_title(self._window, name)

        # Limpa os buffers de cor e profundidade
        r, g, b, a = self.background_color
        GL.glClearColor(r, g, b, a)
        GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT)

    def validate(self, node: Node, model_transformation: np.ndarray) -> bool:
        '''
        Validate a node for rendering.

        Check if the node is compatible to be rendered with this renderer.

        Args:
            node (Node): node to validate.

        Returns:
            bool: True if the node is valid
        '''
        if isinstance(node, Light):
            dummy = np.zeros(4)
            dummy[-1] = 1
            position = model_transformation@dummy
            position = position[:3].astype(np.float32)

            self._lights.append(
                {"node": node, "position": position})
        return ("material" in node.render_data and
                "mesh" in node.render_data)

    def render_valid_node(self, node: Node, model_transformation: np.ndarray):
        '''
        Renders a validated node

        Args:
            node (Node): node to render
            model_transformation (np.ndarray): node model transformation in the scene
        '''
        material: Material = node.render_data["material"]
        mesh: Mesh = node.render_data["mesh"]

        material.use()

        material.shader.set_uniform(
            "modelTransformation",
            model_transformation.astype(np.float32)
        )
        material.shader.set_uniform(
            "viewTransformation",
            self._view_matrix.astype(np.float32)
        )
        material.shader.set_uniform(
            "projectionMatrix",
            self._projection_matrix.astype(np.float32)
        )

        # Envia as informações de cada luz para o shader.
        #
        # O shader espera um array de structs 'light[MAX_LIGHTS]' com os campos:
        #   - type      : inteiro que identifica o tipo da luz
        #                 (e.g. 0 = indefinida, 1 = direcional, 2 = pontual)
        #   - color     : vec3 com a cor/intensidade RGB da luz
        #   - position  : vec3 com a posição em espaço de mundo (luzes pontuais)
        #   - direction : vec3 com a direção em espaço de mundo (luzes direcionais)
        #
        # Apenas as luzes presentes em self._lights são preenchidas; as demais
        # ficam com type = 0 (UNDEFINED), sendo ignoradas pelo shader.
        for i, light_info in enumerate(self._lights):
            light = cast(Light, light_info["node"])
            light_position = cast(np.ndarray, light_info["position"])

            material.shader.set_uniform(f"lights[{i}].type",               light.light_type.value)
            material.shader.set_uniform(f"lights[{i}].color",              light.light_color.astype(np.float32))
            material.shader.set_uniform(f"lights[{i}].direction",          light.light_direction.astype(np.float32))
            material.shader.set_uniform(f"lights[{i}].position",           light_position)
            material.shader.set_uniform(f"lights[{i}].intensity",          float(light.light_intensity))
            material.shader.set_uniform(f"lights[{i}].reference_distance", float(light.light_reference_distance))# Envia a cor ambiente global, somada a todos os fragmentos independente
        # de qualquer luz direcional/pontual.
        material.shader.set_uniform("ambientColor", self.ambient_color)

        mesh.draw()

    def end(self, capture: bool = False):
        '''
        Ends the frame rendering

        Args:
            capture (bool, optional): if should save the current frame. Defaults to False.
        '''
        super().end(capture)

        if capture:
            GL.glPixelStorei(GL.GL_PACK_ALIGNMENT, 1)

            frame_data = GL.glReadPixels(0,  # first pixel x
                                         0,  # first pixel y
                                         self.screen_width,  # dimensão do retângulo sendo lido
                                         self.screen_height,  # dimensão do retângulo sendo lido
                                         GL.GL_BGRA,
                                         GL.GL_UNSIGNED_BYTE)
            frame_data = cast(bytes, frame_data)

            frame = np.frombuffer(frame_data, np.uint8)
            frame = frame.reshape([self.screen_height, self.screen_width, 4])
            frame = np.flipud(frame)

            filename = get_filename_unique(self._name)

            self._executor.submit(save_frame, filename, frame)

        # Troca o buffer frontal e traseiro, mostrando o novo buffer renderizado
        glfw.swap_buffers(self._window)

        glfw.poll_events()

    def should_stop(self) -> bool:
        return glfw.window_should_close(self._window)

    def __del__(self):
        self._executor.shutdown()
        glfw.terminate()