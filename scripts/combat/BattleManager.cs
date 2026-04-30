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
        private MoveResource _move;

        public override void _Ready() 
        {
            _playerCharacters = GetTree().GetNodesInGroup("playercharacters");

            GetMovesFromPlayer();
        }

        private void GetMovesFromPlayer()
        {
            foreach (PlayerCharacter playerCharacter in _playerCharacters)
            {
                _moves = playerCharacter.MovesetComponent.GetMoves();
            }

            _move = _moves[0];
        }

        public MoveResource GetMove() => _move;
    }
}

