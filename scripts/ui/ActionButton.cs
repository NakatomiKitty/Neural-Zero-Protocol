using Godot;

namespace NeuralZeroProtocol.Scripts.Ui;

public enum ActionType { Switch, Moves, Run, Block, Attack, Evade }
public partial class ActionButton : TextureButton
{
    [Export] public ActionType Action;
}
