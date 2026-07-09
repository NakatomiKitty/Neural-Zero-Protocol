using Godot;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Characters;

// This will be used in CombatStateMachine via it's PlayerCharacter Parent.
[GlobalClass]
public partial class ActionValidatorComponent : Node
{
	private Character _character;

	public void Initialize(Character character) => _character = character;

	public bool IsNotExhausted() => 
		!_character.DisabilityComponent.HasDisability(DisabilityTypes.Exhausted);
	
	public bool CanUseMove(MoveResource move) => 
		IsNotExhausted() && _character.MovesetComponent.HasEnoughNrg(move);

    // Can always dodge or block, regardless if Exhausted
    public bool CanDefend() => true;

    public bool CanSkip() => true;
}

