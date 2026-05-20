using Godot;

namespace NeuralZeroProtocol.Scripts.Characters;

[GlobalClass]
public partial class PlayerCharacter : Character
{
    public ActionValidatorComponent ActionValidatorComponent;

    public override void _Ready()
    {
        ActionValidatorComponent = GetNode<ActionValidatorComponent>("ActionValidatorComponent");
        AddToGroup("PlayerCharacters");
    }
}


