using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using RocnikovaPraca.Shaders;

namespace RocnikovaPraca.Objects
{

    public class Skybox
    {
        private float[] vertices =
        {
            // positions
            -1.0f, 1.0f, -1.0f,
            -1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,
            1.0f, 1.0f, -1.0f,
            -1.0f, 1.0f, -1.0f,

            -1.0f, -1.0f, 1.0f,
            -1.0f, -1.0f, -1.0f,
            -1.0f, 1.0f, -1.0f,
            -1.0f, 1.0f, -1.0f,
            -1.0f, 1.0f, 1.0f,
            -1.0f, -1.0f, 1.0f,

            1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,

            -1.0f, -1.0f, 1.0f,
            -1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, -1.0f, 1.0f,
            -1.0f, -1.0f, 1.0f,

            -1.0f, 1.0f, -1.0f,
            1.0f, 1.0f, -1.0f,
            1.0f, 1.0f, 1.0f,
            1.0f, 1.0f, 1.0f,
            -1.0f, 1.0f, 1.0f,
            -1.0f, 1.0f, -1.0f,

            -1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f, 1.0f,
            1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f, 1.0f,
            1.0f, -1.0f, 1.0f
        };

        private int textureID = 0;

        private int vertexArrayObject, vertexBufferObject;

        private Shader shader;

        public Skybox(List<string> faces, Shader shader)
        {
            Initialize(faces, shader);
        }

        private void Initialize(List<string> faces, Shader shader)
        {
            textureID = Utils.BaseUtils.LoadCubemap(faces);

            this.shader = shader;
            
            
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float),
                vertices,
                BufferUsageHint.StaticDraw
            );

            GL.VertexAttribPointer(
                0, 
                3, 
                VertexAttribPointerType.Float, 
                false, 
                3 * sizeof(float), 
                0);
            
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Render(Matrix4 view, Matrix4 projection)
        {
            GL.DepthMask(false);
            
            shader.Use();
            shader.SetMatrix4("view", view);
            shader.SetMatrix4("projection", projection);
            
            GL.BindVertexArray(vertexArrayObject);
            GL.BindTexture(TextureTarget.TextureCubeMap, textureID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.DepthMask(true);
        }

        public Shader GetShader()
        {
            return shader;
        }
    }
}