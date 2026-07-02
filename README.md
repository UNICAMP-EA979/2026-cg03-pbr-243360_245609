[![Open in Visual Studio Code](https://classroom.github.com/assets/open-in-vscode-2e0aaae1b6195c2367325f4f02e2d04e9abb55f0b24a779b69b11b9e10269abc.svg)](https://classroom.github.com/online_ide?assignment_repo_id=24172110&assignment_repo_type=AssignmentRepo)
# PBR

Nesta tarefa iremos implementar um modelo de sombreamento PBR. O modelo irá possuir luz difusa utilizando o modelo de Lambert e luz especular utilizando o modelo de Blinn-Phong. Também irá suportar o uso de luzes direcionais, pontuais (point light) e ambiente.

## Estrutura da atividade

Esta atividade faz parte de uma sequência de atividades de computação gráfica. Elas possuem dependências entre si, onde o código de uma atividade anterior poderá ser reutilizado futuramente.

Os códigos a serem implementados podem estar em duas pastas diferentes: "src" para implementações do pacote do renderizador em si, "entrypoints" para códigos que usarão o renderizador implementado para desenhar cenas. Os entrypoints podem ser úteis para checar as implementações do pacote.

> Procure por trechos com `## SEU CÓDIGO AQUI` em arquivos .py, .vs e .fs

A entrega deve ser pelo GitHub, consistindo tanto do código desenvolvido quanto de imagens e vídeos gerados.

### `#include` em arquivos glsl

Alguns códigos de shaders possuem a diretiva de pré-processador `#include "nome_do_arquivo.glsl"`. Esta diretiva não é padrão da linguagem GLSL e foi implementada para esta atividade dentro do sistema de compilação de shaders (classe `Shader`). A diretiva irá procurar o arquivo indicado dentro da pasta "urenderer/renderer/opengl/shader_library" e substituir a linha da diretiva pelo conteúdo do arquivo, de forma semelhante a mesma diretiva em um código C.

## Tarefas a serem realizadas

Copie os arquivos da tarefa anterior:

- [ ] urenderer/node/node.py
- [ ] urenderer/node/camera.py
- [ ] urenderer/aplication/runtime.py: copie **apenas** as funções que implementou na última tarefa.
- [ ] urenderer/renderer/opengl/texture.py: copiar métodos bind_at_unit e __init__

- [ ] urenderer/renderer/opengl/shader.py: 
  - copiar método use	
  - copiar códigos de compilação e linkagem do shader no __init__ (não copiar método inteiro)

- [ ] urenderer/renderer/opengl/opengl_renderer.py:
  - copiar códigos de inicialização do GLFW, janela e contexto no __init__ (não copiar método inteiro)
  - copiar código de inicialização dos buffers de cor de profundidade no start (não copiar método inteiro)
  - render_valid_node: copiar definição das matrizes de transformação do shader (não copiar método inteiro)
  - copiar o código de swap de buffer do end (não copiar método inteiro)

Os arquivos indicados possuem mais informações quando necessário. Observe que, nos entrypoints, também pode ser necessário editar os arquivos de shaders `.vs` e `.fs`

Observe que todas as atividade irão utilizar o mesmo shader de vértice `vertex.vs`.

Utilize os testes para checar o funcionamento de cada atividade.

### 01 - Luz Básica

Você deve implementar um sombreamento básico considerando apenas a direção e cor das luzes. Você também deve realizar ajuste de gamma (RGB->sRGB) do frame final.

- [ ] urenderer/renderer/opengl/opengl_renderer.py:
  - Realize ajuste de gamma (`__init__`)
  - Envie as informações das luzes na cena para o shader (`render_valid_node`)
- [ ] entrypoints/01-light_direction.py: edite os arquivos indicados pelo entrypoint

Observe que o vetor normal é um vetor de direção, portanto sua representação em coordenadas homogêneas é $[x, y, z, 0]$, e que ele deve ser normalizado tanto antes quanto depois da interpolação do vértice para o fragmento. 

### 02 - Luz difusa

Adicione luz difusa ao modelo de sombreamento.

- [ ] entrypoints/02-diffuse.py: edite os arquivos indicados pelo entrypoint

### 03 - Luz Especular

Adicione luz especular ao modelo de sombreamento.

- [ ] entrypoints/03-specular.py: edite os arquivos indicados pelo entrypoint

### 04 - Luz Ambiente

Adicione luz ambiente constante ao modelo de sombreamento.

- [ ] urenderer/renderer/opengl/opengl_renderer.py: envie a cor ambiente para o shader (`render_valid_node`)
- [ ] entrypoints/04-ambient.py: edite os arquivos indicados pelo entrypoint

### 05 - Materiais

Adicione suporte à texturas para definir os parâmetros da superfície.

- [ ] entrypoints/05-materials.py: edite os arquivos indicados pelo entrypoint

Observe que o entrypoint instancia duas texturas de "base color", ambas com parâmetro `srgb=True`. Isso especifica o formato da textura como sRGB para realizar a correção de gama (sRGB->RGB) antes de ser utilizada.

A atividade solicita para implementar _tiling_ de texturas. Essa é uma técnica que repete uma mesma textura várias vezes sobre a superfície a partir de um fator. Como não definimos um tipo de _wrap_ para a textura, ela irá utilizar `GL_REPEAT`, que repete ela sempre que o valor da coordenada estiver fora do range `[0, 1]`.

**Pergunta**
Após realizar o entrypoint, observe o resultado. São renderizadas 4 esferas, 2 dielétricas, uma metálica e uma entre metálico e dielétrico. A esfere metálica quase não é visível, por quê? Qual técnica vista em aula poderia ser aplicada para melhorar sua renderização?

> Resposta



## Executando o código

1. Instale o pacote na pasta raiz do repositório: `python -m pip install -e .`
   - Observe que o comando instala o pacote no modo editável (opção `-e`), isto significa que qualquer modificação que você fizer nele é diretamente refletida ao utilizar.
2. Execute os entrypoings na pasta "entrypoints": `python xx-nome.py`

Obs: utilize `python` ou `python3` de acordo com seu sistema.

## Correção

Cada atividade vale 20% da nota.

Testes são executados para avaliar as atividades. Você pode testar seu código utilizando o pytest e também verificar sua pontuação no GitHub.

**NÃO ALTERE O CÓDIGO DOS TESTES**

## Informações úteis

### Debugando um shader

Como não é possível inspecionar diretamente o valor de uma variável em um shader, pode ser difícil encontrar o motivo de um comportamento inesperado. Para analisar o seu funcionamento, pode ser útil alterar a cor do fragmento. Você pode utilizar esta técnica quando:

- Precisa descobrir se o código alcançou alguma região ou condição específica:
  - Tente alterar a cor do fragmento para alguma cor chamativa (ex: `FragColor = vec3(1.0, 0.0, 1.0)`) quando essa condição ocorrer.
- Precisa entender o valor de alguma variável:
  - Tente alterar a cor do fragmento para o valor da variável (ex: `FragColor = vec4(worldNormalNormalized, 1.0)`).  

### Equações 

Equações que você pode precisar ao realizar a atividade:

- Equação de Reflexão para luzes pontuais (punctual lights):

$L_o(\vec{p}, \vec{v}) = \pi  \sum_{i=1}^{n} f(\vec{l}_i, \vec{v}) c_i (\vec{p}, \vec{l}_i)(\vec{n}\cdot\vec{l}_i)^{+}$

- Direção da luz:

$\vec{l} = normalize(\vec{p}_{light}-\vec{p}_0)$

- Cor de uma luz direcional:

$c_{light}(r, \vec{l}) = c_{light_0}$

- Cor de uma luz pontual (point light):

$c_{light}(r, \vec{l}) = c_{light_0} ({r_0 \over max(r, r_{min})})^2$

- Half-Angle:

$\vec{h} = {\vec{l}+\vec{v}\over||\vec{l}+\vec{v}||}$

- Aproximação de Schlick para a Equação de Fresnel:

$F(\vec{n}, \vec{l}) \approx F_0+(1-F_0)(1-(\vec{n}\cdot\vec{l})^+)^5$

- Modelo de Lambert para luz difusa (com correção para energia refletida especularmente):
 
$f_{diff}(\vec{l}, \vec{v}) = (1-F(\vec{n}, \vec{l})) {p_{ss}\over\pi}$

- Modelo de Blinn-Phong para luz especular:

$f_{spec}(\vec{l}, \vec{v}) = F(\vec{h}, \vec{l}) {\alpha_p+2\over 8 \pi} {(\vec{n}\cdot\vec{m})^{\alpha_p} \over (\vec{n} \cdot \vec{l})( \vec{n} \cdot \vec{v})}$

$\alpha_p = m^{smoothness}$

- Refletância total

$f(\vec{l}, \vec{v})  = f_{spec}(\vec{l}, \vec{v}) + f_{diff}(\vec{l}, \vec{v})$

- Luz ambiente constante

$L_o(\vec{p}, \vec{v}) = p_{ss}c_{light}(1-metallic)$
