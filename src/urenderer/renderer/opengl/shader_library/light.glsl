#ifndef LIBRARY_LIGHT

const int LIGHT_UNSET = 0;
const int LIGHT_DIRECTIONAL = 1;
const int LIGHT_POINT = 2;
const float R_MIN = 0.05;

struct Light
{
    int type;
    vec3 color;
    float intensity;
    vec3 direction; //Only directional
    vec3 position; //Only point
    float reference_distance; //Only point
};

// Calcula a atenuação da luz
float computeLightAttenuation(Light light, vec3 position)
{
    if(light.type == LIGHT_DIRECTIONAL)
    {
        // Luz direcional: sem atenuação por distância
        return 1.0;
    }

    // Luz pontual: atenuação quadrática pelo inverso da distância.
    //
    //   att = (reference_distance / max(dist, R_MIN))²
    //
    // - Quando dist == reference_distance → att == 1.0 (intensidade plena)
    // - Cai quadraticamente conforme dist cresce (lei do inverso do quadrado)
    // - R_MIN evita divisão por zero / singularidade quando dist → 0
    float dist = length(light.position - position);
    float dClamped = max(dist, R_MIN);
    return (light.reference_distance * light.reference_distance) /
           (dClamped * dClamped);
}

// Calcula a direção da luz (vetor DO fragmento ATÉ a fonte, normalizado)
vec3 computeLightDirection(Light light, vec3 position)
{
    if(light.type == LIGHT_DIRECTIONAL)
    {
        // light.direction aponta DA fonte PARA a cena;
        // invertemos para obter o vetor que aponta até a fonte.
        return normalize(-light.direction);
    }

    // Luz pontual: direção do fragmento até a posição da luz
    return normalize(light.position - position);
}

#define LIBRARY_LIGHT
#endif