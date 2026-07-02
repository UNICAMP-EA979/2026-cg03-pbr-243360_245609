#ifndef LIBRARY_SPECULAR
#define PI 3.14159265359

// Calcula a refletância especular pelo modelo de Blinn-Phong com
// conservação de energia:
//
//   f_specular = fresnel × D(H) / normalizador
//
// onde D(H) é o termo de distribuição de microfacetas de Blinn-Phong:
//
//   D(H) = ((alpha + 2) / (2π)) × max(dot(N, H), 0)^alpha
//
// e o expoente alpha é derivado de roughness:
//
//   alpha = 2 / roughness⁴ - 2
//
// O fator de normalização ((alpha + 2)(alpha + 4)) / (8π(2^(-alpha/2) + alpha))
// é aproximado aqui pelo termo mais simples (alpha + 2) / (2π), suficiente
// para Blinn-Phong energeticamente conservado em tempo real.
//
// Args:
//   fresnel       : vec3 — fração de energia refletida especularmente (de fresnel.glsl)
//   normal        : vec3 — normal da superfície normalizada em espaço de mundo
//   halfAngle     : vec3 — vetor H = normalize(V + L)
//   viewDirection : vec3 — direção do fragmento até a câmera
//   LightDirection: vec3 — direção do fragmento até a fonte de luz
//   roughness     : float — rugosidade da superfície em [0, 1]
//                           0 = espelho perfeito, 1 = superfície totalmente rugosa
vec3 specularReflectance(vec3 fresnel, vec3 normal, vec3 halfAngle, vec3 viewDirection, vec3 LightDirection, float roughness)
{
    // Converte roughness para o expoente de Blinn-Phong.
    // roughness⁴ evita valores extremos e alinha melhor com PBR.
    float r4    = max(roughness * roughness * roughness * roughness, 1e-4);
    float alpha = 2.0 / r4 - 2.0;

    // Cosseno do ângulo entre a normal e o half-angle (NdotH)
    float NdotH = max(dot(normal, halfAngle), 0.0);

    // Distribuição de Blinn-Phong normalizada:
    //   D = (alpha + 2) / (2π) × NdotH^alpha
    float D = ((alpha + 2.0) / (2.0 * PI)) * pow(NdotH, alpha);

    // Denominador de visibilidade geométrica implícita:
    //   divide por 4 × NdotV × NdotL para aproximar o termo geométrico G
    float NdotV = max(dot(normal, viewDirection),  0.0);
    float NdotL = max(dot(normal, LightDirection), 0.0);
    float denom = max(4.0 * NdotV * NdotL, 1e-4);

    // Refletância especular final: F × D / denom
    return fresnel * D / denom;
}

#define LIBRARY_SPECULAR
#endif