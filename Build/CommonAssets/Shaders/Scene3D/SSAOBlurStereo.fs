#version 460
#extension GL_OVR_multiview2 : require
//#extension GL_EXT_multiview_tessellation_geometry_shader : enable

layout(location = 0) out float OutIntensity;
layout(location = 0) in vec3 FragPos;
uniform sampler2DArray Texture0;

void main()
{
    vec2 uv = FragPos.xy;
    if (uv.x > 1.0f || uv.y > 1.0f)
        discard;
    //Normalize uv from [-1, 1] to [0, 1]
    uv = uv * 0.5f + 0.5f;
    
    vec2 texelSize = 1.0f / textureSize(Texture0, 0).xy;
    float result = 0.0f;
    for (int x = -2; x < 2; ++x) 
    {
        for (int y = -2; y < 2; ++y) 
        {
            vec2 offset = vec2(float(x), float(y)) * texelSize;
            result += texture(Texture0, vec3(uv + offset, gl_ViewID_OVR)).r;
        }
    }
    OutIntensity = result / 16.0f;
}