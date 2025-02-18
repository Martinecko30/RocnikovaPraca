using System.Diagnostics;
using EngineBase.Lightning;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using RocnikovaPraca.Engine.Core;
using RocnikovaPraca.Objects;
using RocnikovaPraca.Shaders;

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
    }

    private const bool DEBUG = false;

    private Camera camera;

    private readonly List<GameObject> gameObjects = new();
    private Shader shader; // TODO: shader manager
    private readonly List<Light> lights = new();

    private Stopwatch timer;
    
    private const int SHADOW_WIDTH = 4096;
    private const int SHADOW_HEIGHT = 4096;
    
    private List<(int, int)> depthMaps = new();
    
    private Shader depthShader; // TODO: Fix?
    private Matrix4 lightSpaceMatrix;
    
    private Skybox skybox;


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
        
        /*
        gameObjects.Add(
            new GameObject("res\\Assets\\Models\\Map\\map.obj",
                new Vector3(0, 0, 0),
                new Vector3(0.75f))
            );
        */
        
        /*
        gameObjects.Add(
            new GameObject("Bookshelf",
                "res\\Assets\\Models\\Bookcase\\model.obj",
                new Vector3(0, 0.5f, 0),
                new Vector3(scale))
        );
        */
        gameObjects.Add(
            new GameObject("Forest",
                "res\\Assets\\Models\\Kaykit\\tree_forest.obj",
                new Vector3(0, 0.5f, 0),
                new Vector3(1))
        );
        
        gameObjects.Add(
            new GameObject("Plane",
                "res\\Assets\\Models\\plane.obj",
                new Vector3(0, 0, 0),
                new Vector3(10f))
        );
        
        
        lights.Add(new DirectLight(
            new Vector3(-2f, 10f, -1f),     // Position
            new Vector3(1.0f, 0.95f, 0.8f)    // Color {0.0 - 1.0}
            ));


        for (int i = 0; i < lights.Count; i++)
        {
            if (lights[i].GetType() == typeof(DirectLight))
            {
                (int depthMap, int depthMapFBO) = ((DirectLight) lights[i]).CreateShadowMap(new Vector2i(SHADOW_WIDTH, SHADOW_HEIGHT));
                depthMaps.Add((depthMap, depthMapFBO));
            }
            else if (lights[i].GetType() == typeof(SpotLight))
            {
                
            }
        }

        List<string> skyboxFaces = new List<string>
        {
            "res\\Assets\\Skybox\\right.jpg",
            "res\\Assets\\Skybox\\left.jpg",
            "res\\Assets\\Skybox\\top.jpg",
            "res\\Assets\\Skybox\\bottom.jpg",
            "res\\Assets\\Skybox\\front.jpg",
            "res\\Assets\\Skybox\\back.jpg"
        };

        skybox = new Skybox(
            skyboxFaces, 
            new Shader(
                "res\\Shaders\\SkyboxShader.vert", 
                "res\\Shaders\\SkyboxShader.frag"
                )
            );
        

        camera = new Camera(new Vector3(0, 20, 0), Size.X / (float)Size.Y);
        CursorState = CursorState.Grabbed;
        
        GL.Enable(EnableCap.DepthTest);
        //GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.FramebufferSrgb);

        timer = new Stopwatch();
        timer.Start();
        
        shader.SetBool("gamma", false); // TODO: Gamma correction
        shader.SetInt("depthMap", 2);
        shader.SetInt("skybox", 0);
        
        
        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Lequal);
    }
    
    

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        var timeStart = timer.ElapsedMilliseconds;
        
        //double timeValue = timer.Elapsed.TotalSeconds;
        base.OnRenderFrame(args);
        
        // TODO: Shadow Maps
        foreach (var (map, mapFBO) in depthMaps)
        {
            RenderShadowMap(map, mapFBO);
        }
        
        // Reset viewport
        GL.Viewport(0, 0, Size.X, Size.Y);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        
        // Prepare Shader
        shader.Use();
        
        // Set lights in shader
        for(int i = 0; i < lights.Count; i++)
        {
            if (lights[i].GetType() == typeof(DirectLight))
            {
                
            }
            shader.SetVector3($"lights[{i}].position", lights[i].Position);
            shader.SetVector3($"lights[{i}].color", lights[i].Color);
            shader.SetVector3($"lights[{i}].direction", lights[i].Direction);
        }
        
        
        shader.SetVector3("viewPos", camera.Position);
        
        shader.SetMatrix4("view", camera.GetViewMatrix());
        shader.SetMatrix4("projection", camera.GetProjectionMatrix());
        shader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
        
        shader.SetFloat("near_plane", camera.NearPlane);
        shader.SetFloat("far_plane", camera.FarPlane);
        
        RenderScene(shader);
        
        base.SwapBuffers();
    }

    private void RenderShadowMap(int depthMap, int depthMapFBO)
    {
        GL.CullFace(CullFaceMode.Front);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        float nearPlane = 1.0f, farPlane = 100f;
        Matrix4 lightProjection = Matrix4.CreateOrthographic(40f, 40f, nearPlane, farPlane);
        Matrix4 lightView = Matrix4.LookAt(lights[0].Position, Vector3.Zero, Vector3.UnitY);
        lightSpaceMatrix = lightView * lightProjection;
        
        depthShader.Use();
        depthShader.SetMatrix4("lightSpaceMatrix", lightSpaceMatrix);
        
        
        GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT); // Shadow map TODO: IMPORTANT
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
        
        var shadowMapLocation = shader.GetUniformLocation("shadowMap");
        shader.Use();
        GL.Uniform1(shadowMapLocation, 2); // TODO: Rework so it isn't hardcoded 2 (0 = diffuse, 1 = specular)
        
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.ActiveTexture(TextureUnit.Texture2);
        GL.BindTexture(TextureTarget.Texture2D, depthMap);
        RenderScene(depthShader);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.CullFace(CullFaceMode.Back);
    }

    private void RenderScene(Shader sceneShader)
    {
        foreach (var gameObject in gameObjects)
        {
            //if(IsInViewFrustum(camera.GetViewMatrix(), gameObject)) // TODO: Implement
            //  continue;
            var viewModel = Matrix4.Identity;
            viewModel *= Matrix4.CreateTranslation(gameObject.Position);
            viewModel *= Matrix4.CreateScale(gameObject.Scale);
            sceneShader.SetMatrix4("model", viewModel);
            sceneShader.SetMatrix4("modelInverseTransposed", TransposeAndInverseMatrix(viewModel));
            gameObject.Draw(sceneShader);
            
            if(DEBUG)
                DrawBoundingBox(gameObject.GetBoundingBox());
        }
        
        Matrix4 viewMatrix = new Matrix4(new Matrix3(camera.GetViewMatrix()));
        skybox.Render(viewMatrix, camera.GetProjectionMatrix());
    }

    private void DrawBoundingBox(Box3 boundingBox)
    {
        float[] verts =
        [
            boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z, 
            boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Min.Z,
                
            boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z, 
            boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Min.Z,
                
            boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z, 
            boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.Z,
                
                
                
            boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z, 
            boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Min.Z,
                
            boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z, 
            boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.Z,
                
            boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z, 
            boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z,
            
            
            
            boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Max.Z,
            boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z,
                
            boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Max.Z,
            boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Min.Z,
                
            boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Max.Z,
            boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.Z,
                
                
                
            boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Min.Z,
            boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Min.Z,
                
            boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Min.Z,
            boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z,
                
            boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Min.Z,
            boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Min.Z
        ];
        DrawLines(verts);
    }

    
    // This is suboptimal, we shouldn't create new VBO each draw
    private void DrawLines(float[] linesVerts)
    {
        var linesVAO = GL.GenVertexArray();
        var linesVBO = GL.GenBuffer();
        GL.BindVertexArray(linesVAO);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, linesVBO);
        GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * linesVerts.Length, linesVerts, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        GL.EnableVertexAttribArray(0);
        
        GL.DrawArrays(PrimitiveType.Lines, 0, linesVerts.Length);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    private Matrix4 TransposeAndInverseMatrix(Matrix4 input)
    {
        input = input.Inverted();
        input.Transpose();

        return input;
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

        foreach (var gameObject in gameObjects)
        {
            gameObject.Update();
        }
        
        

        float cameraSpeed = 1.5f;
        const float sensitivity = 0.2f;

        if (input.IsKeyDown(Keys.LeftControl))
        {
            cameraSpeed *= 2f;
        }
        
        

        const float GRAVITY = -9.81f;
        bool cameraCollision = false;
        

        // This works but movement is too fast to detect collisions
        // Either check collisions in-between frames or come up with
        // new smarter way to check for collision
        
        var camPos = camera.Position;// + new Vector3(0, -1.8f, 0);
        
        foreach (var gameObject in gameObjects)
        {
            if (gameObject.CheckCollision(camPos, grounded:cameraCollision))
            {
                cameraCollision = true;
                break;
            }
        }



        if (!cameraCollision)
            camera.Position += (float) (GRAVITY * e.Time) * Vector3.UnitY;

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
        
        if (input.IsKeyDown(Keys.Space) && cameraCollision)
        {
            camera.Position += Vector3.UnitY * cameraSpeed * (float)e.Time * -GRAVITY * 100; // Up
        }
        
        /*
        if (input.IsKeyDown(Keys.Space))
        {
            camera.Position += Vector3.UnitY * cameraSpeed * (float)e.Time; // Up
        }
        if (input.IsKeyDown(Keys.LeftShift))
        {
            camera.Position -= Vector3.UnitY * cameraSpeed * (float)e.Time; // Down
        }
        */
        
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