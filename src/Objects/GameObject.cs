using OpenTK.Mathematics;
using RocnikovaPraca.Shaders;

namespace RocnikovaPraca.Objects;

public class GameObject
{
    private Model model;

    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public GameObject(Model model)
    {
        this.model = model;
    }

    public GameObject(string modelFilePath, Vector3 position)
    {
        this.model = new Model(modelFilePath);
        this.Position = position;
    }
    
    public GameObject(string modelFilePath, Vector3 position, Vector3 scale)
    {
        this.model = new Model(modelFilePath);
        this.Position = position;
        this.Scale = scale;
    }
    
    public GameObject(string modelFilePath)
    {
        this.model = new Model(modelFilePath);
    }
    
    public void Draw(Shader shader)
    {
        model.Draw(shader);
    }
}