#version 330 core
#include "light.glsl"
#define MAX_LIGHT 10
#define PI 3.14159265359

// Realiza o sombreamento considerando dados da luz
// Considera f_brdf = 1

in vec3 worldPosition;
in vec3 worldNormal;
out vec4 FragColor;

uniform Light lights[MAX_LIGHT];

void main()
{
    // Normal renormalizada após interpolação da rasterização
    vec3 worldNormalNormalized = normalize(worldNormal);

    vec3 color = vec3(0);

    for(int i = 0; i < MAX_LIGHT; i++)
    {
        Light light = lights[i];

        if(light.type == LIGHT_UNSET)
        {
            break;
        }

        // ── Atenuação ─────────────────────────────────────────────────────────
        // Direcional: sem atenuação por distância → 1.0
        // Pontual:    atenuação quadrática  att = (referenceDistance / dist)²
        //             quando dist == referenceDistance, att == 1 (intensidade plena)
        float attenuation;
        if(light.type == LIGHT_DIRECTIONAL)
        {
            attenuation = 1.0;
        }
        else // LIGHT_POINT
        {
            float dist      = length(light.position - worldPosition);
            float refDist   = max(light.reference_distance, 0.001);
        }
        attenuation  = computeLightAttenuation(light, worldPosition);
        vec3 lightColor    = light.color * light.intensity * attenuation;
        vec3 lightDirection = computeLightDirection(light, worldPosition);

        // ── Contribuição da luz ───────────────────────────────────────────────
        // Equação de renderização simplificada com f_brdf = 1:
        //
        //   L_o = L_i × f_brdf × cos(θ)
        //       = lightColor × 1 × max(dot(N, L), 0)
        //
        // NdotL (cosseno do ângulo de incidência) garante que luz rasante
        // ou vinda de trás não contribua negativamente.
        float NdotL = max(dot(worldNormalNormalized, lightDirection), 0.0);
        vec3 lightContribution = lightColor * NdotL;

        color += lightContribution;
    }

    // Cor final do fragmento (alpha = 1.0)
    // O ajuste de gamma RGB→sRGB é feito automaticamente via GL_FRAMEBUFFER_SRGB
    FragColor = vec4(color, 1.0);
}