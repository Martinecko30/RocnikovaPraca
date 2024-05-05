namespace RocnikovaPraca.Lightning;

public class SpotLight : Light
{
    // Spotlight specific parameters
    public float CutOff { get; set; }
    public float OuterCutOff { get; set; }
}