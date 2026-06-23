using Godot;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Characters;

// This will be used in CombatStateMachine via it's PlayerCharacter Parent.
[GlobalClass]
public partial class ActionValidatorComponent : Node
{
	private Character _character;

	public void Initialize(Character character) => _character = character;

	public bool IsExhausted() => 
		!_character.DisabilityComponent.HasDisability(DisabilityTypes.Exhausted);
	
	public bool CanUseMove(MoveResource move) => 
		IsExhausted() && !_character.MovesetComponent.CheckNrg(move);

    public bool CanAttack() => 
		IsExhausted() && !_character.DisabilityComponent.HasDisability(DisabilityTypes.Fragile);

    public bool CanDodge() => 
		IsExhausted() && !_character.DisabilityComponent.HasDisability(DisabilityTypes.Stiff);

    public bool CanCrit() =>  
		IsExhausted() && !_character.DisabilityComponent.HasDisability(DisabilityTypes.Mindless);

    // Can always dodge or block, regardless if Exhausted
    public static bool CanDefend() => true;

    public static bool CanSkip() => true;
}

