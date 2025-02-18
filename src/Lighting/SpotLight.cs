using OpenTK.Mathematics;

namespace EngineBase.Lightning;

public class SpotLight : Light
{
    // Spotlight specific parameters
    public float CutOff { get; set; }
    public float OuterCutOff { get; set; }

    public SpotLight(Vector3 position, Vector3 color) : base(position, color)
    { }
}