#version 450

layout (location = 0) out vec3 OutColor;
layout (location = 20) in vec3 FragPosLocal;

uniform sampler2D Texture0;

const float PI = 3.14159265359f;

void main()
{
    vec3 N = normalize(FragPosLocal);

    vec3 irradiance = vec3(0.0f);

    // tangent space calculation from origin point
    vec3 up    = vec3(0.0f, 1.0f, 0.0f);
    vec3 right = cross(up, N);
    up         = cross(N, right);

    float sampleDelta = 0.025f;
    int numSamples = 0;
    for (float phi = 0.0f; phi < 2.0f * PI; phi += sampleDelta)
    {
        for (float theta = 0.0f; theta < 0.5f * PI; theta += sampleDelta)
        {
            // spherical to cartesian (in tangent space)
            float tanX = sin(theta) * cos(phi);
            float tanY = sin(theta) * sin(phi);
            float tanZ = cos(theta);

            vec2 uv = vec2((phi / (2.0f * PI)) + 0.5f, 1.0f - ((theta / PI) + 0.5f));

            irradiance += texture(Texture0, uv).rgb * cos(theta) * sin(theta);
            ++numSamples;
        }
    }

    OutColor = irradiance * vec3(PI / float(numSamples));
}
