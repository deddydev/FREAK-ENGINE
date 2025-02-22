#version 450

layout (location = 0) out vec4 OutColor;
layout (location = 20) in vec3 FragPosLocal;

uniform samplerCube Texture0;

void main()
{
    OutColor = texture(Texture0, FragPosLocal);
}
