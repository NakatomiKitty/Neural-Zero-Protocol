using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class ActionValidatorComponent : Node
    {
        private StatsComponent _statsComponent;
        private DisabilityComponent _disabilityComponent;

        public override void _Ready() 
        {
            _statsComponent = GetNode<StatsComponent>("../StatsComponent");
            _disabilityComponent = GetNode<DisabilityComponent>("../DisabilityComponent");
        }

        // public bool CanAttack()
        // public bool CanDodge()
        // public bool CanCrit()
        // public bool CanTriggerRandomActions()
        // public bool CanBlock()
        // public bool CanSkip()
    }
}
