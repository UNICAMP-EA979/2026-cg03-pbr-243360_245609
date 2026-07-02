#ifndef LIBRARY_DIFUSE
#define PI 3.14159265359

// Calcula a refletância difusa da superfície pelo modelo de Lambert com
// conservação de energia:
//
//   f_diffuse = (1 - F) × (1 - metallic) × baseColor / π
//
// onde:
//   (1 - fresnel)   → energia que NÃO foi refletida especularmente (Fresnel)
//   (1 - metallic)  → metais puros absorvem toda a energia restante como
//                     especular; não possuem lóbulo difuso
//   baseColor / π   → BRDF de Lambert normalizada: divide por π para que a
//                     integral sobre o hemisfério resulte em 1 (energia conservada)
vec3 diffuseReflectance(vec3 fresnel, vec3 baseColor, float metallic)
{
    vec3 kD = (vec3(1.0) - fresnel) * (1.0 - metallic);
    return kD * baseColor / PI;
}

#define LIBRARY_DIFUSE
#endif