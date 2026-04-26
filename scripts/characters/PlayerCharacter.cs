using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class PlayerCharacter : Character
    {
        public ActionValidatorComponent ActionValidatorComponent;

        public override void _Ready() 
        {
            StatsComponent = GetNode<StatsComponent>("StatsComponent");
            DisabilityComponent = GetNode<DisabilityComponent>("DisabilityComponent");
            HealthComponent = GetNode<HealthComponent>("HealthComponent");
            ActionValidatorComponent = GetNode<ActionValidatorComponent>("ActionValidatorComponent");
        }
    }
}

