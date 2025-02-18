using OpenTK.Mathematics;
using RocnikovaPraca.Shaders;

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
        model = new Model(modelFilePath, scale);
        Position = position;
        Scale = scale;
    }
    
    public GameObject(string name, string modelFilePath, Vector3 position, Vector3 scale)
    {
        model = new Model(modelFilePath, scale);
        Position = position;
        Scale = scale;
        Name = name;
    }
    
    public GameObject(string modelFilePath)
    {
        model = new Model(modelFilePath);
    }
    
    public void Draw(Shader shader)
    {
        model.Draw(shader);
    }

    public virtual void Update()
    {
        
    }

    public bool CheckCollision(Vector3 point, bool grounded = true)
    {
        var boundBox = GetBoundingBox();
        
        if (grounded)
            return boundBox.ContainsInclusive(point);
        
        return point.X >= boundBox.Min.X && point.X <= boundBox.Max.X &&
               // point.Y >= boundingBox.Min.Y && point.Y <= boundingBox.Max.Y &&
               point.Z >= boundBox.Min.Z && point.Z <= boundBox.Max.Z &&
               Math.Abs(boundBox.Max.Y - point.Y) < 0.75f;
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
        if (model == null)
            throw new NullReferenceException("Model is null");
        
        return model.GetBoundingBox();
    }
}