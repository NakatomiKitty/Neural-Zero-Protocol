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

        // public bool CanAttack() fail if HasDisability(Fragile)
        // public bool CanDodge() fail if HasDisability(Stiff)
        // public bool CanCrit() fail if HasDisability(Mindless)
        // public bool CanTriggerRandomActions() fail if GetStat(NRG) == 0
        // public bool CanBlock() always true, forced to block if HasDisability(Exhausted)
        // public bool CanSkip() always true
    }
}
