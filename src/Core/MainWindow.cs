#region

using System.Diagnostics;
using EngineBase.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using EngineBase.Lightning;
using RocnikovaPraca.Objects;
using RocnikovaPraca.Shaders;

#endregion

namespace RocnikovaPraca.Core;

public class MainWindow : GameWindow
{
    public MainWindow(GameWindowSettings gameWindowSettings,
        NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
        GLFW.WindowHint(WindowHintInt.Samples, 4);
    }

    ~MainWindow()
    {
        shader.Dispose();
        depthShader.Dispose();
        debugDepthQuad.Dispose();
    }

    private Camera camera;

    private readonly List<GameObject> gameObjects = new List<GameObject>();
    private Shader shader; // TODO: shader manager
    private readonly List<Light> lights = new();

    private Stopwatch timer;
    
    private const int SHADOW_WIDTH = 1024;
    private const int SHADOW_HEIGHT = 1024;
    private int depthMapFBO;
    private int depthMap;
    private Shader depthShader; // TODO: Fix?
    private Shader debugDepthQuad;
    private Matrix4 lightSpaceMatrix;
    
    
    protected override void OnLoad()
    {
        new ShaderManager(new Shader(
            "res\\Shaders\\DefaultShader.vert",
            "res\\Shaders\\DefaultShader.frag"));
        
        base.OnLoad();
        
        GL.Enable(EnableCap.Multisample);

        GL.ClearColor(0.01f, 0.01f, 0.01f, 1.0f);

        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        shader = new Shader(
            "res\\Shaders\\DefaultShader.vert",
            "res\\Shaders\\DefaultShader.frag");

        depthShader = new Shader(
            "res\\Shaders\\DepthShader.vert", 
            "res\\Shaders\\DepthShader.frag"
            );
        
        debugDepthQuad = new Shader(
            "res\\Shaders\\ShadowMappingDepth.vert", 
            "res\\Shaders\\ShadowMappingDepth.frag"
        );

        gameObjects.Add(
            new GameObject("res\\Models\\Minecart.obj",
                new Vector3(0, 0, 0),
                new Vector3(1))
        );
        gameObjects[0].name = "Minecart"; // TODO: Remove
        gameObjects.Add(
            new GameObject("res\\Models\\plane.obj",
                new Vector3(0, 0, 0),
                new Vector3(5))
        );
        gameObjects[1].name = "Plane"; // TODO: Remove
        /*
        gameObjects.Add(new GameObject(
            "res\\Models\\cube2.obj",
            new Vector3(7, 0, 0),
            new Vector3(0.2f, 0.2f, 0.2f)
        ));*/
        
        // Lights
        
        lights.Add(new Light(
            new Vector3(-2f, 4f, 1f), // Position
            new Vector3(1.0f, 1.0f, 1.0f) // Color {0.0 - 1.0}
        ));
        
        /*
        lights.Add(new Light(
                new Vector3(5, 2.0f, 0), 
                new Vector3(1.0f, 1.0f,1.0f)) // Color {0.0 - 1.0}
        );
        */

        CreateShadowMap();

        camera = new Camera(new Vector3(0, 0.333f, 1) * 3, Size.X / (float)Size.Y);
        CursorState = CursorState.Grabbed;
        
        GL.Enable(EnableCap.DepthTest);
        //GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.FramebufferSrgb);

        timer = new Stopwatch();
        timer.Start();
        
        shader.SetBool("gamma", true);
        shader.SetInt("depthMap", 2);
    }
    
    

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        //double timeValue = timer.Elapsed.TotalSeconds;
        base.OnRenderFrame(args);
        
        // TODO: Shadow Map
        RenderShadowMap();
        
        // Reset viewport
        GL.Viewport(0, 0, Size.X, Size.Y);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        /*
        // TODO: Rewrite
        debugDepthQuad.Use();
        debugDepthQuad.SetFloat("near_plane", 1.0f);
        debugDepthQuad.SetFloat("far_plane", 7.5f;
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, depthMap);
        RenderQuad();
        */
        
        
        
        // Prepare Shader
        shader.Use();
        shader.SetVector3("viewPos", camera.Position);
        
        // Set lights in shader
        for(int i = 0; i < lights.Count; i++)
        {
            shader.SetVector3($"lights[{i}].position", lights[i].Position);
            shader.SetVector3($"lights[{i}].color", lights[i].Color);
            shader.SetVector3($"lights[{i}].direction", lights[i].Direction);
        }
        
        shader.SetMatrix4("view", camera.GetViewMatrix());
        shader.SetMatrix4("projection", camera.GetProjectionMatrix());
        shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
        
        RenderScene(shader);
        
        base.SwapBuffers();
    }
    
    private void CreateShadowMap() // TODO: Shadow Map
    {
        // Configure Depth Map
        depthMapFBO = GL.GenFramebuffer();
        depthMap = GL.GenTexture();
        
        GL.BindTexture(TextureTarget.Texture2D, depthMap);
        GL.TexImage2D(
            TextureTarget.Texture2D, 
            0, 
            PixelInternalFormat.DepthComponent, 
            SHADOW_WIDTH, 
            SHADOW_HEIGHT, 
            0,
            PixelFormat.DepthComponent, 
            PixelType.Float, 
            IntPtr.Zero);
        
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
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
    }

    private void RenderShadowMap()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        float nearPlane = 1.0f, farPlane = 7.5f;
        Matrix4 lightProjection = Matrix4.CreateOrthographic(20f, 20f, nearPlane, farPlane);
        Matrix4 lightView = Matrix4.LookAt(lights[0].Position, Vector3.Zero, Vector3.UnitY);
        lightSpaceMatrix = lightView * lightProjection;
        
        depthShader.Use();
        depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
        
        
        GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT); // Shadow map TODO: IMPORTANT
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
        
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, depthMap);
        RenderScene(depthShader);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void RenderScene(Shader sceneShader)
    {
        foreach (var gameObject in gameObjects)
        {
            var viewModel = Matrix4.Identity;
            viewModel *= Matrix4.CreateTranslation(gameObject.Position);
            viewModel *= Matrix4.CreateScale(gameObject.Scale);
            sceneShader.SetMatrix4("model", viewModel);
            sceneShader.SetMatrix4("modelInverseTransposed", TransposeAndInverseMatrix(viewModel));
            gameObject.Draw(sceneShader);
        }
    }

    private Matrix4 TransposeAndInverseMatrix(Matrix4 input)
    {
        input = input.Inverted();
        input.Transpose();

        return input;
    }
    
    
    // TODO: Temporary render Quad
    private int quadVAO;
    private int quadVBO;

    private void RenderQuad()
    {
        if (quadVAO == 0)
        {
            float[] quadVertices =
            {
                // positions        // texture Coords
                -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
                -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
                1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
            };
            quadVAO = GL.GenVertexArray();
            quadVBO = GL.GenBuffer();
            GL.BindVertexArray(quadVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        }
        GL.BindVertexArray(quadVAO);
        GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
        GL.BindVertexArray(0);
    }
    
    
    


    private bool _firstMove = true;
    private Vector2 _lastPos = Vector2.Zero;
    
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused)
        {
            return;
        }

        var input = KeyboardState;

        if (input.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        const float cameraSpeed = 1.5f;
        const float sensitivity = 0.2f;

        
        if (input.IsKeyDown(Keys.W))
        {
            camera.Position += camera.Front * cameraSpeed * (float)e.Time; // Forward
        }
        if (input.IsKeyDown(Keys.S))
        {
            camera.Position -= camera.Front * cameraSpeed * (float)e.Time; // Backwards
        }
        if (input.IsKeyDown(Keys.A))
        {
            camera.Position -= camera.Right * cameraSpeed * (float)e.Time; // Left
        }
        if (input.IsKeyDown(Keys.D))
        {
            camera.Position += camera.Right * cameraSpeed * (float)e.Time; // Right
        }
        if (input.IsKeyDown(Keys.Space))
        {
            camera.Position += camera.Up * cameraSpeed * (float)e.Time; // Up
        }
        if (input.IsKeyDown(Keys.LeftShift))
        {
            camera.Position -= camera.Up * cameraSpeed * (float)e.Time; // Down
        }

        var mouse = MouseState;

        if (_firstMove)
        {
            _lastPos = new Vector2(mouse.X, mouse.Y);
            _firstMove = false;
        }
        else
        {
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;
            _lastPos = new Vector2(mouse.X, mouse.Y);

            camera.Yaw += deltaX * sensitivity;
            camera.Pitch -= deltaY * sensitivity;
        }
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, Size.X, Size.Y);
        // We need to update the aspect ratio once the window has been resized.
        camera.AspectRatio = Size.X / (float)Size.Y;
    }
}