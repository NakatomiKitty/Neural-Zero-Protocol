using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class Character : Node2D
    {
        public StatsComponent StatsComponent;
        public DisabilityComponent DisabilityComponent;
        public ActionValidatorComponent ActionValidatorComponent;

        public override void _Ready() 
        {
            StatsComponent = GetNode<StatsComponent>("StatsComponent");
            DisabilityComponent = GetNode<DisabilityComponent>("DisabilityComponent");
            ActionValidatorComponent = GetNode<ActionValidatorComponent>("ActionValidatorComponent");
        }
    }
}

