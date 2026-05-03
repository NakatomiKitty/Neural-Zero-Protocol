using System;
using Godot;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Cards;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using NeuralZeroProtocol.Scripts.Ui;
using NeuralZeroProtocol.Scripts.UI;

namespace NeuralZeroProtocol.Scripts.Combat
{
    [Scene]
    public partial class BattleScene : Node2D
    {
        [Node] public TurnManager TurnManager;
        [Node] public ActionPanel ActionPanel;
        [Node] public CombatStateMachine CombatStateMachine;
        [Node] public BattleManager BattleManager;
        [Node] public CardSystem CardSystem;
        [Node] public UISelectionController UISelectionController;
        private MoveResource[] _moves;

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
			{
				WireNodes();
			}
        }

        public override void _Ready() 
        {
            foreach (Character character in TurnManager.AllUnits)
			{
                // Links every Character's HealthComponent's Died signal in the battle
				character.HealthComponent.Died += CombatStateMachine.OnCharacterDied;
			}
            
            // Passes the array to Cardsystem so it can be used to update the card skins
            _ = CardSystem.CreateHandFromMoves(BattleManager.GetSelectedMovesFromPlayer());
        }

        public override void _Input(InputEvent @event) 
        {
            CardSystem.GetUIInput(UISelectionController.GetUISelect());
        }
    }
}

