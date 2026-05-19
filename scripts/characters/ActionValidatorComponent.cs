using Godot;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Characters
{
	[GlobalClass]
	public partial class ActionValidatorComponent : Node
	{
		private Character _character;

		public override void _Ready() 
		{
			_character = GetNode<Character>("..");

			if (_character == null)
			{
				GD.PushError($"Character is not loaded in!");
			}
		}

		public bool isExhausted() => 
			!_character.DisabilityComponent.HasDisability(DisabilityTypes.Exhausted);
		
		public bool CanUseMove(MoveResource move) => 
			isExhausted() && !_character.MovesetComponent.CheckNrg(move);

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
