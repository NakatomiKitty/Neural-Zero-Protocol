using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Combat;

public partial class BattleManager : Node
{
    private BattleScene _battleScene;

    private Array<Node> _playerCharacters = new();
    private Array<MoveResource> _moves = new();
    private Array<MoveResource> _selectedMoves;

    public override void _Ready() 
    {
        _playerCharacters = GetTree().GetNodesInGroup("PlayerCharacters");
        
        GetSelectedMovesFromCurrentChar();
    }

    // Gets the moves from the moveset component, then get 5 random moves and returns it as an array
    public Array<MoveResource> GetSelectedMovesFromCurrentChar()
    {
        foreach (PlayerCharacter currentCharacter in _playerCharacters)
        {
            _moves = currentCharacter.MovesetComponent.GetMoves().Duplicate();
        }

        _moves.Shuffle();

        int cardsToTake = Mathf.Min(5, _moves.Count);
        _selectedMoves = _moves.Slice(0, cardsToTake);
        return _selectedMoves;
    }
}


