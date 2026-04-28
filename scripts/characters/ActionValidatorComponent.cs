using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
	[GlobalClass]
	public partial class ActionValidatorComponent : Node
	{
		private Character _character;

		public void Initialize(Character character) => _character = character;

		public bool isExhausted() => 
			!_character.DisabilityComponent.HasDisability(DisabilityTypes.Exhausted);
		
        public bool CanAttack() => 
			isExhausted() && !_character.DisabilityComponent.HasDisability(DisabilityTypes.Fragile);

        public bool CanDodge() => 
			isExhausted() && !_character.DisabilityComponent.HasDisability(DisabilityTypes.Stiff);

        public bool CanCrit() =>  
			isExhausted() && !_character.DisabilityComponent.HasDisability(DisabilityTypes.Mindless);

        // Can always dodge or block, regardless if Exhausted
        public bool CanDefend() => true;

        public bool CanSkip() => true;
    }
}
