using Godot;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Cards;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
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

            CardSystem.UpdateCardSkin(BattleManager.GetMove());
        }
    }
}

