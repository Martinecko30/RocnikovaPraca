using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using RocnikovaPraca.Core;

namespace RocnikovaPraca;

public class Program
{
    public static void Main(string[] args)
    {
        var nativeWindowSettings = new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600),
            Title = "Rocnikova Praca - Martin Valent", // Názov je ľubovoľný
        };
        
            
        using var window = new MainWindow(GameWindowSettings.Default, nativeWindowSettings);
        window.Run();
    }
}