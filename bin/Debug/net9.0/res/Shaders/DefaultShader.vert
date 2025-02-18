#version 330 core
layout (location = 0) in vec3 aPos; // Vertex pos
layout (location = 1) in vec3 aNormal; // Vertex normal
layout (location = 2) in vec2 aTexCoords; // Texture Coordinate

out VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec4 FragPosLightSpace;
} vs_out;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 lightSpaceMatrix;

uniform mat4 modelInverseTransposed;

void main()
{
    vs_out.FragPos = vec3(vec4(aPos, 1.0) * model);
    vs_out.Normal = mat3(modelInverseTransposed) * aNormal;
    vs_out.TexCoords = aTexCoords;
    vs_out.FragPosLightSpace = vec4(aPos, 1.0) * model * lightSpaceMatrix; // TODO: Check if works

    gl_Position = vec4(aPos, 1.0) * model * view * projection;
}