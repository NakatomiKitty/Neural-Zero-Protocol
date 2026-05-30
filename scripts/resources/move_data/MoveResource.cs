using Godot;
using NeuralZeroProtocol.Autoloads;

namespace NeuralZeroProtocol.Scripts.Resources.MoveData;

public enum ActionType
{
    AttackAction,
    SpellAction
}

[GlobalClass]
public partial class MoveResource : Resource
{
    [Export] public ElementType MoveElement = ElementType.None;
    [Export] public ActionType ActionType = ActionType.AttackAction;
    [Export] public Rarity SelectedRarity = Rarity.Scrap;
    [Export] public string Name;
    [Export] public int Power;
    [Export] public int Cost;
    [Export] public bool CanCrit = true;
}


