using Godot;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.UI;
using System;

namespace NeuralZeroProtocol.Scripts.Combat
{
    public partial class BattleScene : Node2D
    {
        public TurnManager TurnManager;
        public ActionPanel ActionPanel;
        public CombatStateMachine CombatStateMachine;
        public BattleManager BattleManager;

        public override void _EnterTree() 
        {
            TurnManager = GetNode<TurnManager>("TurnManager");
            ActionPanel = GetNode<ActionPanel>("ActionPanel");
            CombatStateMachine = GetNode<CombatStateMachine>("CombatStateMachine");
            BattleManager = GetNode<BattleManager>("BattleManager");

            CombatStateMachine.Initialize(this);
            BattleManager.Initialize(this);
        }

        public override void _Ready() 
        {
            foreach (Character character in TurnManager.AllUnits)
			{
                // Links every Character's HealthComponent's Died signal in the battle
				character.HealthComponent.Died += CombatStateMachine.OnCharacterDied;
			}
        }
    }
}

