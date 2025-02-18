using OpenTK.Mathematics;

namespace EngineBase.Lightning;

using OpenTK.Graphics.OpenGL;

public class DirectLight : Light
{
    private int depthMapFBO;
    private int depthMap;

    public DirectLight(Vector3 position, Vector3 color) : base(position, color)
    { }
    
    public DirectLight(Vector3 position, Vector3 color, Vector3 direction) : base(position, color, direction)
    { }
    
    
    
    /// <summary>
    /// Creates Shadow Map of directional light
    /// For example: Sun
    /// </summary>
    /// <param name="resolution">The resolution of the shadow map.</param>
    /// <returns>First int is depthMap texture and second one is depthMapFBO (Framebuffer).</returns>
    public (int, int) CreateShadowMap(Vector2i resolution)
    {
        // Configure Depth Map
        depthMapFBO = GL.GenFramebuffer();
        depthMap = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2D, depthMap);
        GL.TexImage2D(
            TextureTarget.Texture2D, 
            0, 
            PixelInternalFormat.DepthComponent, 
            resolution.X,
            resolution.Y,
            0,
            PixelFormat.DepthComponent, 
            PixelType.Float, 
            IntPtr.Zero);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        
        // Set texture parameters for wrapping mode
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

        // Define the border color
        float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
        
        // attach depth texture as FBO's depth buffer
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
            TextureTarget.Texture2D, depthMap, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        
        // check framebuffer completeness
        FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            Console.WriteLine("Framebuffer Error: " + status);
        }

        // unbind framebuffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        return (depthMap, depthMapFBO);
    }
}