#ifndef LIBRARY_FRESNEL

// Calcula a refletância de Fresnel pela aproximação de Schlick:
//
//   F(θ) = F0 + (1 - F0) × (1 - cos θ)⁵
//
// onde:
//   F0  = refletância a incidência zero
//         → dielétricos: ~0.04 (constante)
//         → metais:      baseColor (a cor do metal é sua refletância especular)
//   cos θ = dot(halfAngle, lightDirection)
//         → ângulo entre H e L; quando a luz incide perpendicular à superfície
//           (θ = 0) a refletância é mínima (F0); quando rasante (θ → 90°)
//           tende a 1.0 independente do material (efeito de borda)
vec3 fresnelReflectance(vec3 baseColor, float metallic, vec3 halfAngle, vec3 lightDirection)
{
    // F0: interpola entre dielétrico (0.04) e metálico (baseColor)
    vec3 F0 = mix(vec3(0.04), baseColor, metallic);

    // cos θ entre o half-angle e a direção da luz, limitado a [0, 1]
    float cosTheta = max(dot(halfAngle, lightDirection), 0.0);

    // Aproximação de Schlick: F0 + (1 - F0) × (1 - cosθ)⁵
    return F0 + (vec3(1.0) - F0) * pow(1.0 - cosTheta, 5.0);
}

#define LIBRARY_FRESNEL
#endif