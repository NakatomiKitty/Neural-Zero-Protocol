using System;
using System.Linq;
using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Cards;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Combat
{
    public partial class BattleManager : Node
    {
        private BattleScene _battleScene;

        private Array<Node> _playerCharacters = new Array<Node>();
        private Array<MoveResource> _moves = new Array<MoveResource>();
        private Array<MoveResource> _selectedMoves;

        public override void _Ready() 
        {
            _playerCharacters = GetTree().GetNodesInGroup("playercharacters");

            GetSelectedMovesFromPlayer();
        }

        // Gets the moves from the moveset component, then get 5 random moves and returns it as an array
        public Array<MoveResource> GetSelectedMovesFromPlayer()
        {
            foreach (PlayerCharacter playerCharacter in _playerCharacters)
            {
                _moves = playerCharacter.MovesetComponent.GetMoves().Duplicate();
            }

            _moves.Shuffle();

            int cardsToTake = Mathf.Min(5, _moves.Count);
            _selectedMoves = _moves.Slice(0, cardsToTake);
            return _selectedMoves;
        }
    }
}

