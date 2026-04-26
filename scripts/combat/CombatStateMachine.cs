using Godot;
using NeuralZeroProtocol.Scripts.Characters;
using System;
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
        [Export] public Node PlayerTeam;
		[Export] public Node EnemyTeam;

        private BattleState _currentState;
        private TurnManager _turnManager;

        public override void _Ready() 
        {
            _turnManager = GetNode<TurnManager>("../TurnManager");

            ChangeState(BattleState.Initializing);
        }

        public async void ChangeState(BattleState newState)
        {
            _currentState = newState;
            EmitSignal(SignalName.StateChanged, (int)newState);

            switch (_currentState)
            {
                case BattleState.Initializing:
                    Initializing();
                    break;
                case BattleState.PlayerTurn:
                    await PlayerTurn();
                    break;
                case BattleState.EnemyTurn:
                    await EnemyTurn();
                    break;
                case BattleState.TurnEnd:
                    TurnEnd();
                    break;
                case BattleState.Victory:
                    Victory();
                    break;
                case BattleState.Defeat:
                    Defeat();
                    break;
            }
        }

        private void Initializing()
        {
            GD.Print("Battle Initializing...");

            foreach (Character character in _turnManager.AllUnits)
			{
                // Links every Character's HealthComponent's Died signal in the battle
				character.HealthComponent.Died += OnCharacterDied;
			}

            _turnManager.StartBattle();

            var currentCharacter = _turnManager.GetCurrentUnit();

            if (currentCharacter is PlayerCharacter) ChangeState(BattleState.PlayerTurn);

            else if (currentCharacter is EnemyCharacter) ChangeState(BattleState.EnemyTurn);
        }

        private async Task PlayerTurn()
        {
            GD.Print("Player Turn");
        }

        private async Task EnemyTurn()
        {
            GD.Print("Enemy Turn");
        }

        private void TurnEnd()
        {
            GD.Print("Turn End");
        }

        private void Victory()
        {
            GD.Print("Victory");
        }

        private void Defeat()
        {
            GD.Print("Defeat");
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

            // Removes the dead character from AllUnits so it wont show up.
            // NOTE: If you want to add a character that doesnt die when everyone is not dead
            // You should add a flag here that checks if that character cant die.
			if (_turnManager.AllUnits.Contains(deadCharacter)) _turnManager.AllUnits.Remove(deadCharacter);

			// Disconnect the signal to avoid memory leaks
    		deadCharacter.HealthComponent.Died -= OnCharacterDied;
		}
    }
}
