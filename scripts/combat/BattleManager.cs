using System.Collections.Generic;
using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Combat;

public partial class BattleManager : Node
{

    // Gets the moves from the moveset component, then get 5 random moves and returns it as an array
    public Array<MoveResource> GetSelectedMovesFromCurrentChar(PlayerCharacter currentCharacter)
    {
        Array<MoveResource> moves = currentCharacter.MovesetComponent.GetMoves().Duplicate();

        moves.Shuffle();

        int cardsToTake = Mathf.Min(5, moves.Count); 
        
        return moves.Slice(0, cardsToTake);
    }
}


