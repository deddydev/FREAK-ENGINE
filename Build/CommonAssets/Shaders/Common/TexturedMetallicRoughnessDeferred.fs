#version 450

layout (location = 0) out vec4 AlbedoOpacity;
layout (location = 1) out vec3 Normal;
layout (location = 2) out vec4 RMSI;

layout (location = 1) in vec3 FragNorm;
layout (location = 2) in vec3 FragBinorm;
layout (location = 3) in vec3 FragTan;
layout (location = 4) in vec2 FragUV0;

uniform sampler2D Texture0; // Albedo
uniform sampler2D Texture2; // Metallic
uniform sampler2D Texture3; // Roughness

uniform vec3 BaseColor = vec3(1.0f, 1.0f, 1.0f);
uniform float Opacity = 1.0f;
uniform float Specular = 1.0f;
uniform float Roughness = 0.0f;
uniform float Metallic = 0.0f;
uniform float Emission = 0.0f;

void main()
{
    Normal = normalize(FragNorm);
    AlbedoOpacity = vec4(texture(Texture0, FragUV0).rgb * BaseColor, Opacity);

    float metallicTex = texture(Texture2, FragUV0).r;
    float roughnessTex = texture(Texture3, FragUV0).r;
    RMSI = vec4(Roughness * roughnessTex, Metallic * metallicTex, Specular, Emission);
}
