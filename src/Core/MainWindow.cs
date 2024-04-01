using System.Diagnostics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using RocnikovaPraca.Objects;
using RocnikovaPraca.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using RocnikovaPraca.Lightning;

namespace RocnikovaPraca.Core;

public class MainWindow : GameWindow
{
    public MainWindow(GameWindowSettings gameWindowSettings, 
        NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings) {}

    private Camera camera;

    private List<GameObject> gameObjects = new List<GameObject>();
    //private List<Shader> shaders = new List<Shader>();
    private Shader shader; // TODO: shader manager
    private List<Light> lights = new();

    private Stopwatch timer;

    protected override void OnLoad()
    {
        base.OnLoad();
        
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        shader = new Shader(
            "res\\Shaders\\DefaultShader.vert",
            "res\\Shaders\\DefaultShader.frag");
        
        gameObjects.Add(new GameObject("res\\Models\\minecart.obj"));
        
        /*
        gameObjects.Add(new GameObject(
            "res\\Models\\cube2.obj"
            ));*/
        /*
        gameObjects.Add(new GameObject(
            "res\\Models\\cube2.obj",
            new Vector3(7, 0, 0),
            new Vector3(0.2f, 0.2f, 0.2f)
        ));
        */
        
        
        lights.Add(new Light(
            new Vector3(0, 10, 0), 
            new Vector3(1.0f, 1.0f, 1.0f)) // Color {0.0 - 1.0}
        );
        
        /*
        lights.Add(new Light(
                new Vector3(0, -10, 0), 
                new Vector3(1.0f, 1.0f,1.0f)) // Color {0.0 - 1.0}
        );
        */

        camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
        CursorState = CursorState.Grabbed;
        
        //GL.Enable(EnableCap.DepthTest);
        //GL.DepthFunc(DepthFunction.Always);
        GL.Enable(EnableCap.FramebufferSrgb);

        timer = new Stopwatch();
        timer.Start();
    }
    
    

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        double timeValue = timer.Elapsed.TotalSeconds;
        base.OnRenderFrame(args);
        
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        shader.SetVector3("viewPos", camera.Position);
        
        
        for(int i = 0; i < lights.Count; i++)
        {
            Console.WriteLine(lights[i].Position);
            shader.SetVector3($"lights[{i}].position", lights[i].Position);
            shader.SetVector3($"lights[{i}].color", lights[i].Color);
        }
        
        shader.SetMatrix4("view", camera.GetViewMatrix());
        shader.SetMatrix4("projection", camera.GetProjectionMatrix());

        foreach (var gameObject in gameObjects)
        {
            var viewModel = Matrix4.Identity;
            viewModel *= Matrix4.CreateTranslation(gameObject.Position);
            viewModel *= Matrix4.CreateScale(gameObject.Scale);
            shader.SetMatrix4("model", viewModel);
            shader.SetMatrix4("modelInverseTransposed", TransposeAndInverseMatrix(viewModel));
            gameObject.Draw(shader);
        }
        
        base.SwapBuffers();
    }

    private Matrix4 TransposeAndInverseMatrix(Matrix4 input)
    {
        /*
        Vector4 i0 = input.Row0;
        Vector4 i1 = input.Row0;
        Vector4 i2 = input.Row0;
        Vector4 i3 = input.Row0;

        return new Matrix4(
            i0.X, i1.X, i2.X, i3.X,
            i0.Y, i1.Y, i2.Y, i3.Y,
            i0.Z, i1.Z, i2.Z, i3.Z,
            i0.W, i1.W, i2.W, i3.W
        );
        */

        input.Inverted().Transpose();

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