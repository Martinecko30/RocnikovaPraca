using OpenTK.Mathematics;

namespace RocnikovaPraca.Lightning;

public class Light
{
    public Vector3 Position = Vector3.Zero;
    public Vector3 Direction = Vector3.Zero;
    public Vector3 Color = Vector3.One;
    public float Intensity { get; set; }

    // Attenuation parameters
    public float Constant { get; set; }
    public float Linear { get; set; }
    public float Quadratic { get; set; }

    public Light() { }
    
    public Light(Vector3 position)
    {
        this.Position = position;
    }
    
    public Light(Vector3 position, Vector3 color)
    {
        this.Position = position;
        this.Color = color;
    }
    
    public Light(Vector3 position, Vector3 color, Vector3 direction)
    {
        this.Position = position;
        this.Color = color;
        this.Direction = direction;
    }
}
