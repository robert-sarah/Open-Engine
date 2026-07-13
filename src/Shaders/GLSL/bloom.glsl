// Created By Levi Enama
// Bloom Post-Processing Shader
#version 450 core

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D image;
uniform sampler2D bloomBlur;
uniform float exposure;
uniform float bloomStrength;

void main()
{
    vec3 hdrColor = texture(image, TexCoords).rgb;
    vec3 bloomColor = texture(bloomBlur, TexCoords).rgb;

    // Additive blending
    hdrColor += bloomColor * bloomStrength;

    // Tone mapping (Reinhard)
    vec3 result = hdrColor / (hdrColor + vec3(1.0));

    // Gamma correction
    result = pow(result, vec3(1.0 / 2.2));

    FragColor = vec4(result, 1.0);
}
