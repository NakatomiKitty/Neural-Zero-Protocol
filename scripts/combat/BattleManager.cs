using System;
using System.Linq;
using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Characters;

namespace NeuralZeroProtocol.Scripts.Combat
{
    public partial class BattleManager : Node
    {
        private BattleScene _battleScene;

        private Array<Node> _playerCharacters = new Array<Node>();

        public override void _Ready() 
        {
            _playerCharacters = GetTree().GetNodesInGroup("playercharacters");

            foreach (PlayerCharacter playerCharacter in _playerCharacters)
            {
                GD.Print(playerCharacter.MovesetComponent.GetMoves());
            }
        }
    }
}

