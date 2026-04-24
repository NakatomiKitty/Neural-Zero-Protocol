using Godot;
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

		public bool CanAttack() => !_disabilityComponent.HasDisability(DisabilityTypes.Fragile);
		public bool CanDodge() => !_disabilityComponent.HasDisability(DisabilityTypes.Stiff);
		public bool CanCrit() => !_disabilityComponent.HasDisability(DisabilityTypes.Mindless);
		public bool IsForcedToBlock() => _disabilityComponent.HasDisability(DisabilityTypes.Exhausted);
		public bool CanTriggerRandomActions() => _statsComponent.GetStat(StatTypes.Nrg) > 0;
		public bool CanBlock() => true; 
		public bool CanSkip() => true; 
	}
}
