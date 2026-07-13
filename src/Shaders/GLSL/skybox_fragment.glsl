// Created By Levi Enama
// Skybox Fragment Shader
#version 450 core

in vec3 TexCoords;
out vec4 FragColor;

uniform samplerCube skybox;

void main()
{
    FragColor = texture(skybox, TexCoords);
}
