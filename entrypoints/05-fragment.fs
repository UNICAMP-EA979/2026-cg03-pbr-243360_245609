#version 330 core

#include "light.glsl"
#include "fresnel.glsl"
#include "diffuse.glsl"
#include "specular.glsl"

#define MAX_LIGHT 10
#define PI 3.14159265359

// Adicione suporte para texturas com tiling

in vec3 worldPosition;
in vec3 worldNormal;
in vec2 uv;

uniform vec3 ambientColor;
uniform sampler2D baseColorTexture;
uniform sampler2D metallicTexture;
uniform sampler2D roughnessTexture;
uniform float tiling = 1.0;

out vec4 FragColor;

uniform Light lights[MAX_LIGHT];

void main()
{
    // Calcule a normal do fragmento
    vec3 worldNormalNormalized = normalize(worldNormal);

    // Calcule a direção de visualização (saindo do ponto)
    vec3 viewDirection = normalize(-worldPosition);

    // Calcule a uv com tiling
    vec2 uvTiling = uv * tiling;

    // Realize sampling das texturas para obter as propriedades da superfície
    vec3 baseColor = texture(baseColorTexture, uvTiling).rgb;
    float metallic = texture(metallicTexture, uvTiling).r;
    float roughness = max(texture(roughnessTexture, uvTiling).r, 0.05); // Clamp leve para evitar divisão por zero

    vec3 color = vec3(0);

    // Calcule a luz ambiente
    vec3 ambientLightContribution = baseColor * ambientColor * (1.0 - metallic);

    for(int i = 0; i < MAX_LIGHT; i++)
    {
        Light light = lights[i];
        if(light.type == LIGHT_UNSET)
        {
            break;
        }

        //Calcule dados da luz (atenuação, cor, direção)
        float attenuation = (light.type == 2) ? pow(light.reference_distance / max(distance(light.position, worldPosition), light.reference_distance), 2.0) : 1.0;
        vec3 lightColor = light.color * light.intensity * attenuation;
        vec3 lightDirection = (light.type == 1) ? normalize(-light.direction) : normalize(light.position - worldPosition);

        //Calcule o half-angle
        vec3 halfAngle = normalize(lightDirection + viewDirection);

        //Calcule as refletância de fresnel, difusa e especular
        vec3 fresnel = mix(vec3(0.04), baseColor, metallic) + (1.0 - mix(vec3(0.04), baseColor, metallic)) * pow(1.0 - max(dot(lightDirection, halfAngle), 0.0), 5.0);
        vec3 diffuse = (1.0 - fresnel) * (baseColor / PI) * (1.0 - metallic);
        vec3 specular = fresnel * ((pow(2.0 / (roughness * roughness), 2.0) + 2.0) / (8.0 * PI)) * pow(max(dot(worldNormalNormalized, halfAngle), 0.0), pow(2.0 / (roughness * roughness), 2.0));

        //Calcule a refletância final
        vec3 reflectance = diffuse + specular;

        //Calcule a contribuição da luz e acumule na color
        vec3 lightContribution = reflectance * lightColor * max(dot(worldNormalNormalized, lightDirection), 0.0);
        color += lightContribution;
    }

    FragColor = vec4(ambientLightContribution + color, 1.0);
}