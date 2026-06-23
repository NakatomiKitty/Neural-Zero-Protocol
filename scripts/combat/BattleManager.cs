using System.Collections.Generic;
using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Combat;

public partial class BattleManager : Node
{
    private BattleScene _battleScene;
    
    private Array<MoveResource> _selectedMoves;

    // Gets the moves from the moveset component, then get 5 random moves and returns it as an array
    public Array<MoveResource> GetSelectedMovesFromCurrentChar(List<Character> playerCharacters)
    {
        Array<MoveResource> moves = new();
        
        foreach (var character in playerCharacters)
        {
            var currentCharacter = (PlayerCharacter)character;
            moves = currentCharacter.MovesetComponent.GetMoves().Duplicate();
        }

        moves.Shuffle();

        int cardsToTake = Mathf.Min(5, moves.Count);
        _selectedMoves = moves.Slice(0, cardsToTake);
        return _selectedMoves;
    }
}


