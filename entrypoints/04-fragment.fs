#version 330 core

#include "light.glsl"
#include "fresnel.glsl"
#include "diffuse.glsl"
#include "specular.glsl"

#define MAX_LIGHT 10
#define PI 3.14159265359

in vec3 worldPosition;
in vec3 worldNormal;

out vec4 FragColor;

uniform Light lights[MAX_LIGHT];
uniform vec3 ambientColor;

void main()
{
    //normaliza os vetores no espaço
    vec3 worldNormalNormalized = normalize(worldNormal);
    vec3 viewDirection = normalize(-worldPosition); 
    vec3 baseColor = vec3(0.5, 0.2, 0.5);
    float metallic = 0.0;
    float roughness = 0.25;

    vec3 color = vec3(0.0);

    // luz ambiente constante
    vec3 ambientLightContribution = baseColor * ambientColor * (1.0 - metallic);

    for(int i = 0; i < MAX_LIGHT; i++)
    {
        Light light = lights[i];
        if(light.type == LIGHT_UNSET)
        {
            break;
        }

        float attenuation = 1.0;
        vec3 lightDirection = vec3(0.0);

        //direção e atenuaçãp
        if (light.type == 1) {
            lightDirection = normalize(-light.direction);
            attenuation = 1.0; 
        } 
        else if (light.type == 2) { 
            lightDirection = normalize(light.position - worldPosition);
            float r = distance(light.position, worldPosition);
            float r_min = light.reference_distance;
            //inverso do quadrado para decaimento da luz
            attenuation = pow(r_min / max(r, r_min), 2.0);
        }

        vec3 lightColor = light.color * light.intensity * attenuation;

        vec3 halfAngle = normalize(lightDirection + viewDirection);
        float NdotL = max(dot(worldNormalNormalized, lightDirection), 0.0);
        float NdotV = max(dot(worldNormalNormalized, viewDirection), 0.0);
        float NdotH = max(dot(worldNormalNormalized, halfAngle), 0.0);
        float LdotH = max(dot(lightDirection, halfAngle), 0.0); 

        //FRESNEL: Aproximação de Schlick
        // F0 é a refletância base: 0.04 para dielétricos, ou a própria cor base para metais
        vec3 F0 = mix(vec3(0.04), baseColor, metallic);
        vec3 fresnel = F0 + (1.0 - F0) * pow(1.0 - LdotH, 5.0);

        //LUZ DIFUSA
        // energia que não é refletida especularmente (1 - fresnel) refrata e vira luz difusa.
        vec3 diffuse = (1.0 - fresnel) * (baseColor / PI) * (1.0 - metallic);

        // ESPECULAR
        // convertendo roughness para um expoente de brilho (alpha_p)
        float alpha_p = pow(2.0 / (roughness * roughness + 0.001), 2.0); 
        vec3 specular = fresnel * ((alpha_p + 2.0) / (8.0 * PI)) * pow(NdotH, alpha_p);
        vec3 reflectance = diffuse + specular;

        // acumula na cor total
        vec3 lightContribution = reflectance * lightColor * NdotL;
        color += lightContribution;
    }
    FragColor = vec4(ambientLightContribution + color, 1.0);
}