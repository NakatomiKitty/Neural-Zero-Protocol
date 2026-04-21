using Godot;


namespace NeuralZeroProtocol.Stats
{
    [GlobalClass]
    public partial class DeadzoneEffect : Resource
    {
        [Export] public Stat stat {get; set;}
        [Export] public int ThresholdMin {get; set;} = 0;
        [Export] public string DisplayName {get; set;}
        [Export] public string Description {get; set;}
    }
}
