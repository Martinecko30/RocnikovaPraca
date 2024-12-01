#region

using EngineBase.Objects;
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

    public string Name = "default";

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

    public void Update()
    {
        
    }

    public bool CheckCollision(Vector3 point)
    {
        var boundingBox = GetBoundingBox();
        var collisionX = (boundingBox.Min.X >= point.X && 
                          boundingBox.Min.X <= point.X) ||
                         (point.X >= boundingBox.Min.X &&
                          point.X <= boundingBox.Max.X);
        
        var collisionY = (boundingBox.Min.Y >= point.Y && 
                          boundingBox.Min.Y <= point.Y) ||
                         (point.Y >= boundingBox.Min.Y &&
                          point.Y <= boundingBox.Max.Y);
        
        var collisionZ = (boundingBox.Min.Z >= point.Z && 
                          boundingBox.Min.Z <= point.Z) ||
                         (point.Z >= boundingBox.Min.Z &&
                          point.Z <= boundingBox.Max.Z);
        
        return collisionX && collisionY && collisionZ;
    }

    public bool CheckCollision(GameObject gameObject)
    {
        var firstBoundingBox = GetBoundingBox();
        var secondBoundingBox = gameObject.GetBoundingBox();

        var collisionX = (firstBoundingBox.Min.X >= secondBoundingBox.Min.X && 
                          firstBoundingBox.Min.X <= secondBoundingBox.Max.X) ||
                         (secondBoundingBox.Min.X >= firstBoundingBox.Min.X &&
                          secondBoundingBox.Min.X <= firstBoundingBox.Max.X);
        
        var collisionY = (firstBoundingBox.Min.Y >= secondBoundingBox.Min.Y && 
                          firstBoundingBox.Min.Y <= secondBoundingBox.Max.Y) ||
                         (secondBoundingBox.Min.Y >= firstBoundingBox.Min.Y &&
                          secondBoundingBox.Min.Y <= firstBoundingBox.Max.Y);
        
        var collisionZ = (firstBoundingBox.Min.Z >= secondBoundingBox.Min.Z && 
                          firstBoundingBox.Min.Z <= secondBoundingBox.Max.Z) ||
                         (secondBoundingBox.Min.Z >= firstBoundingBox.Min.Z &&
                          secondBoundingBox.Min.Z <= firstBoundingBox.Max.Z);
        
        return collisionX && collisionY && collisionZ;
    }
    
    public Box3 GetBoundingBox()
    {
        return model.GetBoundingBox();
    }
}