using Godot;

namespace NeuralZeroProtocol.Scripts.Characters;
[GlobalClass]
public partial class EnemyCharacter : Character
{
    public override void _Ready() 
    {
        base._Ready();
        
        StatsComponent = GetNode<StatsComponent>("StatsComponent");
        DisabilityComponent = GetNode<DisabilityComponent>("DisabilityComponent");
        HealthComponent = GetNode<HealthComponent>("HealthComponent");
    }
    
}


