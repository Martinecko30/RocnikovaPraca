#version 330 core
#define MAX_LIGHTS 10

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

uniform Light lights[MAX_LIGHTS];
uniform vec3 viewPos;
uniform bool gamma;

uniform float near_plane;
uniform float far_plane;

float LinearizeDepth(float depth)
{
    float z = depth * 2.0 - 1.0; // Back to NDC 
    return (2.0 * near_plane * far_plane) / (far_plane + near_plane - z * (far_plane - near_plane));
    
    /*
    float depthValue = texture(depthMap, TexCoords).r;
    FragColor = vec4(vec3(LinearizeDepth(depthValue) / far_plane), 1.0); // perspective
    // FragColor = vec4(vec3(depthValue), 1.0); // orthographic
    */
}

vec3 BlinnPhong(vec3 normal, vec3 fragPos, vec3 lightPos, vec3 lightColor, float shadow) {
    // Ambient
    vec3 ambient = 0.15 * lightColor;

    // Diffuse
    vec3 lightDir = normalize(lightPos - fragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * lightColor;

    // Specular
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

    // Combine ambient, diffuse, and specular with shadow calculation
    return (ambient + ((1.0 - shadow) * (diffuse + specular)));
}

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal, vec3 fragPos, vec3 lightPos) {
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = (projCoords * 0.5) + 0.5;

    if(projCoords.z > 1.0)
        return 0.0;

    float closestDepth = texture(shadowMap, projCoords.xy).r;
    float currentDepth = projCoords.z;

    
    vec3 lightDir = normalize(lightPos - fragPos);
    // Calculate shadow bias
    float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.005);
    // Check whether current frag pos is in shadow
    float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;
    
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
        }
    }
    shadow /= 9.0;

    return shadow;
}

void main() {
    vec3 color = texture(diffuseTexture, fs_in.TexCoords).rgb;
    vec3 normal = normalize(fs_in.Normal);

    vec3 lighting = vec3(0.0);
    for(int i = 0; i < MAX_LIGHTS; ++i) { // Assuming the number of active lights is less than MAX_LIGHTS
        float shadow = ShadowCalculation(fs_in.FragPosLightSpace, normal, fs_in.FragPos, lights[i].position); // Calculate shadow for each light
        lighting += BlinnPhong(normal, fs_in.FragPos, lights[i].position, lights[i].color, shadow);
    }

    // Apply the lighting result to the texture color
    color *= lighting;

    // Gamma correction
    if(gamma)
        color = pow(color, vec3(1.0 / 2.2));

    FragColor = vec4(color, 1.0);
}