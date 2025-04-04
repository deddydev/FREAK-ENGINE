#version 460
#extension GL_OVR_multiview2 : require
//#extension GL_EXT_multiview_tessellation_geometry_shader : enable

layout(location = 0) out vec3 BloomColor;
layout(location = 0) in vec3 FragPos;

uniform sampler2DArray HDRSceneTex; // HDR color from Deferred & Forward passes
uniform float BloomIntensity = 1.0f;
uniform float BloomThreshold = 1.0f;
uniform float SoftKnee = 0.5f;
uniform vec3 Luminance = vec3(0.299f, 0.587f, 0.114f);

void main()
{
    vec2 uv = FragPos.xy;
    if (uv.x > 1.0f || uv.y > 1.0f)
        discard;
    //Normalize uv from [-1, 1] to [0, 1]
    uv = uv * 0.5f + 0.5f;
    vec3 uvi = vec3(uv, gl_ViewID_OVR);

    vec3 hdr = texture(HDRSceneTex, uvi).rgb;
    float brightness = dot(hdr, Luminance);
    float knee = BloomThreshold * SoftKnee;
    float weight = clamp((brightness - BloomThreshold + knee) / (2.0 * knee + 1e-5), 0.0f, 1.0f);
    weight = weight * weight;
    
    BloomColor = hdr * weight * BloomIntensity;
}
