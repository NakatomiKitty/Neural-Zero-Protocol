using Godot;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.UI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NeuralZeroProtocol.Scripts.Combat
{
    public enum BattleState
    {
        Initializing,
        PlayerTurn,
        EnemyTurn,
        TurnEnd,
        Victory,
        Defeat,
    }

    public partial class CombatStateMachine : Node
    {
        [Signal] public delegate void StateChangedEventHandler(BattleState newState);

        private BattleState _currentState;
        private TurnManager _turnManager;
        private ActionPanel _actionPanel;

        private Character _currentCharacter;

        public async override void _Ready() 
        {
            _turnManager = GetNode<TurnManager>("../TurnManager");
            _actionPanel = GetNode<ActionPanel>("../ActionPanel");

            await ChangeState(BattleState.Initializing);
        }

        public async Task ChangeState(BattleState newState)
        {
            _currentState = newState;
            EmitSignal(SignalName.StateChanged, (int)newState);

            switch (_currentState)
            {
                case BattleState.Initializing:
                    await Initializing();
                    break;
                case BattleState.PlayerTurn:
                    await PlayerTurn();
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
            }
        }

        private async Task Initializing()
        {
            GD.Print("Battle Initializing...");

            _turnManager.StartBattle();
            
            foreach (Character character in _turnManager.AllUnits)
			{
                // Links every Character's HealthComponent's Died signal in the battle
				character.HealthComponent.Died += OnCharacterDied;
			}

            _currentCharacter = _turnManager.GetCurrentUnit();

            await CurrentCharacterTurn(_currentCharacter);
        }

        private async Task PlayerTurn()
        {
            PlayerCharacter playerCharacter = (PlayerCharacter)_turnManager.GetCurrentUnit();
            ActionValidatorComponent actionValidator = playerCharacter.ActionValidatorComponent;

            bool isActionValid = false;
            while (!isActionValid)
            {
                GD.Print($"{playerCharacter.Name}'s turn");
                GD.Print("Press an action");

                SignalAwaiter awaiter = ToSignal(_actionPanel, ActionPanel.SignalName.ActionSelected);
                await awaiter;

                ActionType action = (ActionType)awaiter.GetResult()[0].AsInt32();

                // PLACEHOLDER! MIGHT CHANGE LOGIC
                switch (action)
                {
                    case ActionType.Attack:
                        if (actionValidator.CanAttack())
                        {
                            GD.Print("Attacked!");
                            isActionValid = true;
                        }

                        else
                        {
                            GD.Print("Cant Attack, try again");
                        }
                        break;
                    case ActionType.Defend:
                        if (actionValidator.CanDefend())
                        {
                            GD.Print("Defended!");
                            isActionValid = true;
                        }
                        break;
                    case ActionType.Skip:
                        if (actionValidator.CanSkip())
                        {
                            GD.Print("Skipped!");
                            isActionValid = true;
                        }
                        break;
                }
            }

            // After a valid action, check battle outcome and end turn
            if (BattleOutcomeState()) return;
            await ChangeState(BattleState.TurnEnd);
        }

        // PLACEHOLDER CODE HERE! WILL PROBABLY CONTAIN CALLING TO THE ENEMY'S AI
        private async Task EnemyTurn()
        {
            EnemyCharacter enemyCharacter = (EnemyCharacter)_turnManager.GetCurrentUnit();
            GD.Print($"{enemyCharacter.Name}'s turn");

            GD.Print("Enemy did something!");

            await ChangeState(BattleState.TurnEnd);
        }

        private async Task TurnEnd()
        {
            GD.Print($"{_currentCharacter.Name} ended it's turn!");

            _currentCharacter = _turnManager.AdvanceToNextUnit();

            await CurrentCharacterTurn(_currentCharacter);
        }

        private void Victory()
        {
            GD.Print("Victory");

            foreach (Character character in _turnManager.AllUnits)
			{
                // Delinks every Character's HealthComponent's Died signal in the battle
				character.HealthComponent.Died -= OnCharacterDied;
			}
        }

        private void Defeat()
        {
            GD.Print("Defeat");

            foreach (Character character in _turnManager.AllUnits)
			{
                // Delinks every Character's HealthComponent's Died signal in the battle
				character.HealthComponent.Died -= OnCharacterDied;
			}
        }

        private void OnCharacterDied(Character deadCharacter)
		{
			GD.Print(deadCharacter.Name);

			if (_turnManager.TurnOrder.Contains(deadCharacter))
			{
				int deadIndex = _turnManager.TurnOrder.IndexOf(deadCharacter); 
                // Stores the index of the dead character ↑ 
                // Then removes it ↓
        		_turnManager.TurnOrder.Remove(deadCharacter);
                
                // if deadIndex is before the CurrentUnitIndex, decrement so whoever comes next takes it's place
				if (deadIndex < _turnManager.CurrentUnitIndex) _turnManager.CurrentUnitIndex--;

                // if deadIndex IS the CurrentUnitIndex, immediately go to the next Unit
				else if (deadIndex == _turnManager.CurrentUnitIndex) _turnManager.CurrentUnitIndex++;
			} 
            
            // NOTE: If you want to add a character that doesnt die when everyone is not dead
            // You should add a flag here that checks if that character cant die.
			if (_turnManager.AllUnits.Contains(deadCharacter))
            {
                if (deadCharacter is PlayerCharacter) _turnManager.PlayerCharacters.Remove(deadCharacter);

                if (deadCharacter is EnemyCharacter) _turnManager.EnemyCharacters.Remove(deadCharacter);
            } 

            if (BattleOutcomeState()) return;

			// Disconnect the signal to avoid memory leaks
    		deadCharacter.HealthComponent.Died -= OnCharacterDied;
		}

        // Helper Functions
        private bool BattleOutcomeState()
        {
            if (!_turnManager.PlayerCharacters.Any())
            {
                _ = ChangeState(BattleState.Defeat);
                return true;
            } 
            else if (!_turnManager.EnemyCharacters.Any())
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
}
