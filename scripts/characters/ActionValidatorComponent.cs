using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;

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

        public bool IsExhausted() => !_disabilityComponent.HasDisability(DisabilityTypes.Exhausted); // If not Exhausted, return true
        public bool CanAttack() => IsExhausted() && !_disabilityComponent.HasDisability(DisabilityTypes.Fragile); // If not Exhausted and is not Fragile, CanAttack
        public bool CanDodge() => IsExhausted() && !_disabilityComponent.HasDisability(DisabilityTypes.Stiff); // if not Exhausted and is not Stiff, CanDodge
        public bool CanCrit() => IsExhausted() && !_disabilityComponent.HasDisability(DisabilityTypes.Mindless); // if not Exhausted and is not Mindless, Dodge
        // Can always dodge or block, regardless if Exhausted
        public bool CanBlock() => true;
        public bool CanSkip() => true;
    }
}
