using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using RocnikovaPraca.Shaders;

namespace RocnikovaPraca.Objects;

public class Mesh
{
    public List<Vertex> Vertices { private set; get; }
    public List<uint> Indices { private set; get; }
    public List<Texture> Textures { private set; get; }

    private int VAO, VBO, EBO;
    
    public Mesh(List<Vertex> vertices, List<uint> indices, List<Texture> textures)
    {
        this.Vertices = vertices;
        this.Indices = indices;
        this.Textures = textures;

        SetupMesh();
    }

    private void SetupMesh()
    {
        VAO = GL.GenVertexArray();
        VBO = GL.GenBuffer();
        EBO = GL.GenBuffer();
        
        GL.BindVertexArray(VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        
        GL.BufferData(
            BufferTarget.ArrayBuffer,
            Vertices.Count * Unsafe.SizeOf<Vertex>(),
            Vertices.ToArray(),
            BufferUsageHint.StaticDraw
            );
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
        GL.BufferData(
            BufferTarget.ElementArrayBuffer, 
            Indices.Count * sizeof(uint),
            Indices.ToArray(),
            BufferUsageHint.StaticDraw
            );
        
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            Unsafe.SizeOf<Vertex>(),
            0
            );
        
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(
            1,
            3,
            VertexAttribPointerType.Float,
            false,
            Unsafe.SizeOf<Vertex>(),
            Marshal.OffsetOf<Vertex>(nameof(Vertex.Normal))
        );
        
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(
            2,
            2,
            VertexAttribPointerType.Float,
            false,
            Unsafe.SizeOf<Vertex>(),
            Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoords))
        );
        
        GL.BindVertexArray(0);
    }

    public void Draw(Shader shader)
    {
        uint diffuseNr = 1;
        uint specularNr = 1;

        for (uint i = 0; i < Textures.Count; i++)
        {
            GL.ActiveTexture((TextureUnit)((int)All.Texture0 + i));

            string number = "";
            string name = Textures[(int) i].type;
            if (name == "texture_diffuse")
                number = (diffuseNr++).ToString();
            if (name == "texture_specular")
                number = (specularNr++).ToString();
            
            //"material." + 
            shader.SetInt((name + number), (int)i);
            GL.BindTexture(TextureTarget.Texture2D, Textures[(int) i].textureID);
        }
        
        GL.ActiveTexture(TextureUnit.Texture0);
        
        shader.Use();
        
        GL.BindVertexArray(VAO);
        GL.DrawElements(
            PrimitiveType.Triangles, 
            Indices.Count, 
            DrawElementsType.UnsignedInt, 
            0);
        GL.BindVertexArray(0);
    }
}