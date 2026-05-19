using Godot;
using NeuralZeroProtocol.Scripts.Cards;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using NeuralZeroProtocol.Scripts.Ui;
using NeuralZeroProtocol.Scripts.UI;

namespace NeuralZeroProtocol.Scripts.Combat;

public partial class BattleScene : Node2D
{
    public TurnManager TurnManager;
    public ActionPanel ActionPanel;
    public CombatStateMachine CombatStateMachine;
    public BattleManager BattleManager;
    public CardSystem CardSystem;
    public UiSelectionController UiSelectionController;
    private MoveResource[] _moves;

    public override void _Ready() 
    {
        TurnManager = GetNode<TurnManager>("TurnManager");
        ActionPanel = GetNode<ActionPanel>("ActionPanel");
        CombatStateMachine = GetNode<CombatStateMachine>("CombatStateMachine");
        BattleManager = GetNode<BattleManager>("BattleManager");
        CardSystem = GetNode<CardSystem>("CardSystem");
        
        foreach (Character character in TurnManager.AllUnits)
		{
            // Links every Character's HealthComponent's Died signal in the battle
			character.HealthComponent.Died += CombatStateMachine.OnCharacterDied;
		}

        TurnManager.StartBattle += OnBattleStart;
        
        // Passes the array to CardSystem so it can be used to update the card skins
        _ = CardSystem.CreateHandFromMoves(BattleManager.GetSelectedMovesFromCurrentChar());
    }
    
    public override void _Input(InputEvent @event) 
    {
        CardSystem.GetUiInput(UiSelectionController.GetUiSelect());
    }

    private void OnBattleStart()
    {
        Character firstUnit = TurnManager.GetCurrentUnit();
        CombatStateMachine.SetCurrentCharacter(firstUnit);
        CombatStateMachine.StartBattle();
    }
}


