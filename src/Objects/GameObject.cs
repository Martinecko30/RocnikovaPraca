#region

using EngineBase.Shaders;
using OpenTK.Mathematics;

#endregion

namespace RocnikovaPraca.Objects;

public class GameObject
{
    private readonly Model model;

    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public string name = "default";

    public GameObject(Model model)
    {
        this.model = model;
    }

    public GameObject(string modelFilePath, Vector3 position)
    {
        model = new Model(modelFilePath);
        Position = position;
    }
    
    public GameObject(string modelFilePath, Vector3 position, Vector3 scale)
    {
        model = new Model(modelFilePath);
        Position = position;
        Scale = scale;
    }
    
    public GameObject(string modelFilePath)
    {
        model = new Model(modelFilePath);
    }
    
    public void Draw(Shader shader)
    {
        model.Draw(shader);
    }
}