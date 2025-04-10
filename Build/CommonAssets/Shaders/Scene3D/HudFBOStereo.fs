#version 460
#extension GL_OVR_multiview2 : require
//#extension GL_EXT_multiview_tessellation_geometry_shader : enable

layout (location = 0) out vec4 OutColor;
layout (location = 0) in vec3 FragPos;

uniform sampler2DArray Texture0;

void main()
{
    vec2 uv = FragPos.xy;
    vec3 uvi = vec3(uv, gl_ViewID_OVR);
    OutColor = texture(Texture0, uvi);
}
