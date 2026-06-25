#version 330 core
#include "light.glsl"
#include "fresnel.glsl"
#include "diffuse.glsl"
#include "specular.glsl"
#define MAX_LIGHT 10
#define PI 3.14159265359

// Adicione luz especular ao modelo de sombreamento

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
    float metallic  = 0.0;
    float roughness = 0.25;
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

        // Direção do fragmento até a fonte de luz
        vec3 lightDirection = computeLightDirection(light, worldPosition);

        // Half-angle: bissetor entre V e L — eixo da microfaceta que
        // reflete L diretamente em direção a V
        vec3 halfAngle = normalize(viewDirection + lightDirection);

        // Fresnel (Schlick): quanto da energia vai para o especular
        vec3 fresnel = fresnelReflectance(baseColor, metallic, halfAngle, lightDirection);

        // Difuso de Lambert com conservação de energia:
        // (1 - F)(1 - metallic) × baseColor / π
        vec3 diffuse = diffuseReflectance(fresnel, baseColor, metallic);

        // Especular de Blinn-Phong normalizado:
        // F × D(H) / (4 × NdotV × NdotL)
        vec3 specular = specularReflectance(fresnel, worldNormalNormalized, halfAngle,
                                            viewDirection, lightDirection, roughness);

        // Refletância total: difusa + especular
        vec3 reflectance = diffuse + specular;

        // Contribuição da luz: modula pela cor e pelo NdotL (lei de Lambert)
        float NdotL = max(dot(worldNormalNormalized, lightDirection), 0.0);
        vec3 lightContribution = reflectance * lightColor * NdotL;

        color += lightContribution;
    }

    // O ajuste de gamma RGB→sRGB é feito via GL_FRAMEBUFFER_SRGB no renderer
    FragColor = vec4(color, 1.0);
}