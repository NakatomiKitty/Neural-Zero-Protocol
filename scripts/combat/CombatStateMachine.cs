using System;
using Godot;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Characters;
using System.Linq;
using System.Threading.Tasks;
using NeuralZeroProtocol.Scripts.Ui;
using ActionType = NeuralZeroProtocol.Scripts.Ui.ActionType;

namespace NeuralZeroProtocol.Scripts.Combat;

public enum BattleState
{
    Initializing,
    PlayerTurn,
    EnemyTurn,
    TurnEnd,
    Victory,
    Defeat,
}

[Scene]
public partial class CombatStateMachine : Node
{
    public event Action<BattleState> StateChanged;
    public event Action AttackTriggered;
    
    [Node("ConfirmAttackHandler")]public ConfirmAttackHandler CharacterAttackHandler; 
    
    private BattleScene _battleScene;
    private Character _currentCharacter;
    private BattleState _currentState;
    
    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated) WireNodes();
    }
    
    public void Initialize(BattleScene battleScene) => _battleScene = battleScene;

    public async void StartBattle() => await ChangeState(BattleState.Initializing);
    
    public void SetCurrentCharacter(Character character) => _currentCharacter = character;
    
    public async Task ChangeState(BattleState newState)
    {
        _currentState = newState;
        
        StateChanged?.Invoke(newState);

        switch (_currentState)
        {
            case BattleState.Initializing:
                await Initializing();
                break;
            case BattleState.PlayerTurn:
                PlayerTurn();
                break;
            case BattleState.EnemyTurn:
                await EnemyTurn();
                break;
            case BattleState.TurnEnd:
                await TurnEnd();
                break;
            case BattleState.Victory:
                Victory();
                break;
            case BattleState.Defeat:
                Defeat();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task Initializing()
    {
        GD.Print("Battle Initializing...");
        
        await CurrentCharacterTurn(_currentCharacter);
    }

    private async void PlayerTurn()
    {
        // Reminder: Decouple this shit later
        PlayerCharacter playerCharacter = (PlayerCharacter)_battleScene.TurnManager.GetCurrentUnit();

        EnemyCharacter frontEnemyCharacter = (EnemyCharacter)_battleScene.TurnManager.EnemyCharacters[0];
        
        SignalAwaiter awaiter = ToSignal(_battleScene.BattleMenu, BattleMenu.SignalName.ActionSelected);
        await awaiter;

        ActionType action = (ActionType)awaiter.GetResult()[0].AsInt32();

        if (action == ActionType.Moves)
        {
            GD.Print("Pressed Moves, Should go to the Action Menu.");
            PlayerTurn();
            return;
        }
        
        GD.Print($"{playerCharacter.Name}'s turn");
        
        GD.Print($"Action Selected: {action}");
        
        switch (action)
        {
            // Initial menu
            case ActionType.Switch:
                GD.Print("Bring up the character switch menu");
                break;
            case ActionType.Run:
                GD.Print("run away from the battle (add an animation and chance here probably)");
                break;
            
            // Action Menu
            case ActionType.Block:
                GD.Print("Block Stance!");
                break;
            case ActionType.Attack:
                CharacterAttackHandler.SelectedCardData(playerCharacter, frontEnemyCharacter);
                AttackTriggered?.Invoke();
                break;
            case ActionType.Evade:
                GD.Print("EvadeStance!");
                break;
        }
        
        // W.I.P, WILL HAVE IT MAKE IT GO TO TurnEnd AFTER A MENU ACTION
        // After a valid action, check battle outcome and end turn
        // if (BattleOutcomeState()) return;
        // await ChangeState(BattleState.TurnEnd);
    }

    // PLACEHOLDER CODE HERE! WILL PROBABLY CONTAIN CALLING TO THE ENEMY'S AI
    private async Task EnemyTurn()
    {
        EnemyCharacter enemyCharacter = (EnemyCharacter)_battleScene.TurnManager.GetCurrentUnit();
        
        GD.Print($"{enemyCharacter.Name}'s turn");

        GD.Print("Enemy did something!");

        await ChangeState(BattleState.TurnEnd);
    }

    private async Task TurnEnd()
    {
        GD.Print($"{_currentCharacter.Name} ended it's turn!");

        // Decouple this shit later
        _currentCharacter = _battleScene.TurnManager.AdvanceToNextUnit();

        await CurrentCharacterTurn(_currentCharacter);
    }

    private void Victory()
    {
        GD.Print("Victory");

        foreach (Character character in _battleScene.TurnManager.AllUnits)
		{
            // Delinks every Character's HealthComponent's Died signal in the battle
			character.HealthComponent.Died -= OnCharacterDied;
		}
    }

    private void Defeat()
    {
        GD.Print("Defeat");

        foreach (Character character in _battleScene.TurnManager.AllUnits)
		{
            // Delinks every Character's HealthComponent's Died signal in the battle
			character.HealthComponent.Died -= OnCharacterDied;
		}
    }

    public void OnCharacterDied(Character deadCharacter)
	{
        // DECOUPLE A LOT OF THIS!
		GD.Print(deadCharacter.Name);

		if (_battleScene.TurnManager.TurnOrder.Contains(deadCharacter))
		{
			int deadIndex = _battleScene.TurnManager.TurnOrder.IndexOf(deadCharacter); 
            // Stores the index of the dead character ↑ 
            // Then removes it ↓
        	_battleScene.TurnManager.TurnOrder.Remove(deadCharacter);
            
            // if deadIndex is before the CurrentUnitIndex, decrement so whoever comes next takes it's place
			if (deadIndex < _battleScene.TurnManager.CurrentUnitIndex) _battleScene.TurnManager.CurrentUnitIndex--;

            // if deadIndex IS the CurrentUnitIndex, immediately go to the next Unit
			else if (deadIndex == _battleScene.TurnManager.CurrentUnitIndex) _battleScene.TurnManager.CurrentUnitIndex++;
		} 
        
        // NOTE: If you want to add a character that doesnt die when everyone is not dead
        // You should add a flag here that checks if that character cant die.
		if (_battleScene.TurnManager.AllUnits.Contains(deadCharacter))
        {
            if (deadCharacter is PlayerCharacter) _battleScene.TurnManager.PlayerCharacters.Remove(deadCharacter);

            if (deadCharacter is EnemyCharacter) _battleScene.TurnManager.EnemyCharacters.Remove(deadCharacter);
        } 

        if (BattleOutcomeState()) return;

		// Disconnect the signal to avoid memory leaks
    	deadCharacter.HealthComponent.Died -= OnCharacterDied;
	}

    // Helper Functions
    private bool BattleOutcomeState()
    {
        // Good lord so many decoupling to be done
        if (!_battleScene.TurnManager.PlayerCharacters.Any())
        {
            _ = ChangeState(BattleState.Defeat);
            return true;
        } 
        else if (!_battleScene.TurnManager.EnemyCharacters.Any())
        {
            _ = ChangeState(BattleState.Victory);
            return true;
        } 

        return false;
    }

    private async Task CurrentCharacterTurn(Character character)
    {
        if (character is PlayerCharacter) await ChangeState(BattleState.PlayerTurn);
        else if (character is EnemyCharacter) await ChangeState(BattleState.EnemyTurn);
    }
}
