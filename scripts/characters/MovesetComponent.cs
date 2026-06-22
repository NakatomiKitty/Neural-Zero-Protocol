using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;

namespace NeuralZeroProtocol.Scripts.Characters;

[GlobalClass]
public partial class MovesetComponent : Node
{
    [Export] public Array<MoveResource> Moves = new();
    
    private Character _character;

    public override void _Ready() 
    {
        _character = GetNode<Character>("..");
	}
    
    public Array<MoveResource> GetMoves() => Moves;

    public bool CheckNrg(MoveResource move) => _character.StatsComponent.GetStat(StatType.Nrg) >= move.Cost;
    
    public void SpendNrg(MoveResource move)
    {
        if (CheckNrg(move)) _character.StatsComponent.ModifyStat(StatType.Nrg, -move.Cost);
    }
}

