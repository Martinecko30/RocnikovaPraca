#version 330 core

out vec4 FragColor;

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
    vec4 FragPosLightSpace;
} fs_in;

struct Light {
    vec3 position;
    vec3 color;
};

uniform sampler2D diffuseTexture;
uniform sampler2D specularTexture;

uniform sampler2D shadowMap;

uniform Light lights[1];  // Using only lights[0]
uniform vec3 viewPos;
uniform bool gamma;

// Blinn-Phong lighting function
vec3 BlinnPhong(vec3 normal, vec3 fragPos, vec3 lightPos, vec3 lightColor, float shadow) {
    // Ambient component
    vec3 ambient = 0.15 * lightColor;

    // Diffuse component
    vec3 lightDir = normalize(lightPos - fragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * lightColor;

    // Specular component
    vec3 viewDir = normalize(viewPos - fragPos);
    vec3 halfwayDir = normalize(lightDir + viewDir);
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 64.0);
    vec3 specular = spec * lightColor;

    // Simple attenuation
    float max_distance = 1.5;
    float distance = length(lightPos - fragPos);
    float attenuation = 1.0 / (gamma ? distance * distance : distance);

    // Apply attenuation
    diffuse *= attenuation;
    specular *= attenuation;

    // Combine ambient, diffuse, and specular with shadow factor
    vec3 result = ambient + (1.0 - shadow) * (diffuse + specular);
    return result;
}

// Shadow calculation function
float ShadowCalculation(vec4 fragPosLightSpace) {
    // Perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    // Transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;

    // Get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(shadowMap, projCoords.xy).r;
    // Get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    // Check whether current frag pos is in shadow
    float shadow = currentDepth > closestDepth ? 1.0 : 0.0;

    return shadow;
}

void main() {
    // Texture color
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    // Normal vector
    vec3 normal = normalize(fs_in.Normal);
    // Light properties
    vec3 lightColor = lights[0].color;
    vec3 lightPos = lights[0].position;

    // Calculate shadow for the fragment
    float shadow = ShadowCalculation(fs_in.FragPosLightSpace);

    // Calculate lighting using Blinn-Phong model
    vec3 lighting = BlinnPhong(normal, fs_in.FragPos, lightPos, lightColor, shadow);

    // Apply the lighting result to the texture color
    color *= lighting;

    // Gamma correction
    if (gamma)
        color = pow(color, vec3(1.0 / 2.2));

    // Final fragment color
    FragColor = vec4(color, 1.0);
}
