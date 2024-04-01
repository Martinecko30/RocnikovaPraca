#version 330 core
#define MAX_LIGHTS 10

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

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
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    // ambient
    vec3 ambient = 0.05 * color;
    // diffuse
    vec3 lightDir = normalize(lights[0].position - fs_in.FragPos);
    vec3 normal = normalize(fs_in.Normal);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * color;
    // specular
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    vec3 reflectDir = reflect(-lightDir, normal);
    float spec = 0.0;

    vec3 halfwayDir = normalize(lightDir + viewDir);
    spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    vec3 specular = vec3(0.3) * spec; // assuming bright white light color
    FragColor = vec4(ambient + diffuse + specular, 1.0);
    
    
    /*
    vec3 viewDir = normalize(viewPos - fs_in.FragPos);
    vec3 normal = normalize(fs_in.Normal);
    
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    
    // ambient
    vec3 ambient = 0.05 * color;
    
    vec3 result = vec3(0.0);

    for(int i = 0; i < MAX_LIGHTS; i++) {
        // diffuse
        vec3 lightDir = normalize(lights[i].position - fs_in.FragPos);

        float diff = max(dot(lightDir, normal), 0.0);
        vec3 diffuse = diff * color;

        // specular
        vec3 reflectDir = reflect(-lightDir, normal);
        vec3 halfwayDir = normalize(lightDir + viewDir);
        
        float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

        vec3 specular = lights[0].color * spec * vec3(texture(specularTexture, fs_in.TexCoords));
        
        result += (ambient + diffuse) + specular;
    }

    FragColor = vec4(result, 1.0);
    
    /*
    vec3 result = vec3(0.0);
    vec3 norm = normalize(Normal);
    vec3 viewDir = normalize(viewPos - FragPos);

    for(int i = 0; i < MAX_LIGHTS; i++) {
        vec3 lightDir = normalize(lights[i].position - FragPos);
        float diff = max(dot(norm, lightDir), 0.0);

        vec3 halfwayDir = normalize(lightDir + viewDir);
        float spec = pow(max(dot(norm, halfwayDir), 0.0), 64.0);

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
    */
}