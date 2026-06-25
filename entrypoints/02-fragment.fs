#version 330 core
#include "light.glsl"
#include "fresnel.glsl"
#include "diffuse.glsl"
#define MAX_LIGHT 10
#define PI 3.14159265359

// Adicione luz difusa ao modelo de sombreamento

in vec3 worldPosition;
in vec3 worldNormal;
out vec4 FragColor;

uniform Light lights[MAX_LIGHT];
uniform vec3 cameraPosition;

void main()
{
    // Normal renormalizada após interpolação da rasterização
    vec3 worldNormalNormalized = normalize(worldNormal);

    // Direção de visualização: do fragmento em direção à câmera
    vec3 viewDirection = normalize(cameraPosition - worldPosition);

    vec3 baseColor = vec3(0.5, 0.2, 0.5);
    float metallic = 0.0;
    vec3 color = vec3(0.0);

    for(int i = 0; i < MAX_LIGHT; i++)
    {
        Light light = lights[i];

        if(light.type == LIGHT_UNSET)
        {
            break;
        }

        // Atenuação (1.0 para direcionais, quadrática para pontuais)
        float attenuation = computeLightAttenuation(light, worldPosition);

        // Cor da luz ponderada pela intensidade e atenuação
        vec3 lightColor = light.color * light.intensity * attenuation;

        // Direção do fragmento até a fonte de luz (normalizada)
        vec3 lightDirection = computeLightDirection(light, worldPosition);

        // Half-angle: bissetor entre viewDirection e lightDirection
        // Usado como H na aproximação de Schlick: dot(H, L) mede o
        // quanto a microfaceta está alinhada com a direção de reflexão
        vec3 halfAngle = normalize(viewDirection + lightDirection);

        // Fresnel (Schlick): fresnelReflectance(baseColor, metallic, H, L)
        vec3 fresnel = fresnelReflectance(baseColor, metallic, halfAngle, lightDirection);

        // Difuso de Lambert com conservação de energia:
        // diffuseReflectance já aplica (1-F)(1-metallic) × baseColor/π
        vec3 diffuse = diffuseReflectance(fresnel, baseColor, metallic);

        // Refletância total: difusa + especular (Fresnel)
        // O NdotL modula ambos os termos conforme a lei de Lambert
        float NdotL = max(dot(worldNormalNormalized, lightDirection), 0.0);
        vec3 reflectance = diffuse + fresnel;

        // Contribuição da luz acumulada
        vec3 lightContribution = reflectance * lightColor * NdotL;
        color += lightContribution;
    }

    // O ajuste de gamma RGB→sRGB é feito via GL_FRAMEBUFFER_SRGB no renderer
    FragColor = vec4(color, 1.0);
}