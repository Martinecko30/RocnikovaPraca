namespace RocnikovaPraca.Shaders;

public class ShaderManager
{
    private Dictionary<string, Shader> shaders;

    public ShaderManager(Shader defaultShader)
    {
        shaders = new Dictionary<string, Shader>();
        AddShader("default", defaultShader);
    }
    
    public void AddShader(string name, Shader shader)
    {
        shaders.Add(name, shader);
    }

    public Shader GetShader(string name)
    {
        if (shaders.TryGetValue(name, out Shader shader))
            return shader;
        
        return shaders["default"];
    }
}