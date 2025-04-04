#version 450

layout (location = 0) out vec4 OutColor;
layout (location = 0) in vec3 FragPos;

uniform sampler2D Texture0;

void main()
{
    OutColor = texture(Texture0, FragPos.xy);
}
