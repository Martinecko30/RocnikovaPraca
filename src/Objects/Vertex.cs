using OpenTK.Mathematics;

namespace RocnikovaPraca.Objects;

public struct Vertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoords;

    public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords)
    {
        this.Position = position;
        this.Normal = normal;
        this.TexCoords = texCoords;
    }
}