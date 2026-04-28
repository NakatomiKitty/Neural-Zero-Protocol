using Godot;
using NeuralZeroProtocol.Scripts.UI;
using System;

namespace NeuralZeroProtocol.Scripts.Combat
{
    public partial class BattleManager : Node
    {
        private BattleScene _battleScene;

        public void Initialize(BattleScene battleScene) => _battleScene = battleScene;
    }
}

