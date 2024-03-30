#version 330 core
#define MAX_LIGHTS 10

out vec4 FragColor;

in vec2 TexCoords;
in vec3 Normal;
in vec3 FragPos;

struct Light {
    vec3 position;
    vec3 color;
};

uniform sampler2D diffuseTexture;
uniform sampler2D specularTexture;
uniform Light lights[MAX_LIGHTS];

uniform vec3 viewPos;

void main()
{
    vec3 result = vec3(0.0);
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    for(int i = 0; i < MAX_LIGHTS; i++) {
        vec3 lightDir = normalize(lights[i].position - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);

        vec3 halfwayDir = normalize(lightDir + viewDir);
        float spec = pow(max(dot(norm, halfwayDir), 0.0), 64);

        vec3 diffuse = diff * vec3(texture(diffuseTexture, TexCoords));
        vec3 specular = spec * vec3(texture(specularTexture, TexCoords));

        if (length(diffuse) < 0.00001) { // Almost zero
            diffuse = vec3(0.5f); // Set to half
        }
        if (length(specular) < 0.00001) { // Almost zero
            specular = vec3(0.5f); // Set to half
        }

        result += (diffuse + specular) * lights[i].color;
    }

    //FragColor = vec4(result, 1.0);
    FragColor = texture(diffuseTexture, TexCoords);
}