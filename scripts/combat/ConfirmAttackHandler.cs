using Godot;
using NeuralZeroProtocol.Autoloads;
using NeuralZeroProtocol.Scripts.Cards;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Combat;

public partial class ConfirmAttackHandler : Node
{
	private Card _selectedCard;

	public void SelectedCardData(Character  playerCharacter, EnemyCharacter chosenEnemy)
	{
		GD.Print($"Card Power: {_selectedCard.MoveData.Power}");
		GD.Print($"Card Cost: {_selectedCard.MoveData.Cost}");
		
		if (_selectedCard.MoveData.ActionType == ActionType.SpellAction)
		{
			// Placeholder, they don't exactly do damage
			
			GD.Print($"{playerCharacter} inflicted {_selectedCard.MoveData.Name} to {chosenEnemy}!");
			return;
		}
		
		(bool isDodged, bool isEnemyImmune, bool isCrit, int damageDealt) = DamageCalculator.CalculateDamage(playerCharacter, chosenEnemy, _selectedCard.MoveData);

		if (isDodged)
		{
			GD.Print($"{chosenEnemy.Name} has dodged the attack!");
			return;
		}
		
		if (isEnemyImmune)
		{
			GD.Print($"{chosenEnemy.Name} was immune to the attack!");
			return;
		}
		
		chosenEnemy.HealthComponent.TakeDamage(damageDealt);
		GD.Print($"{playerCharacter.Name} dealt {damageDealt} to {chosenEnemy.Name}!");
		
		if (isCrit)
		{
			GD.Print("It was a critical Hit!");
		}
	}
	
	
	public void SetCurrentSelectedCard(Card selectedCard)
	{
		_selectedCard = null;
		
		_selectedCard = selectedCard;
	}
}
