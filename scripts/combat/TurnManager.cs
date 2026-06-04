using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;

namespace NeuralZeroProtocol.Scripts.Characters;

public partial class TurnManager : Node
{
	public event Action StartBattle;
	
	[Export] public Node PlayerTeam;
	[Export] public Node EnemyTeam;
	[Export] public Timer TurnTimer;

	public List<Character> TurnOrder = new();

	public List<Character> PlayerCharacters;
	public List<Character> EnemyCharacters;
	public List<Character> AllUnits => PlayerCharacters.Concat(EnemyCharacters).ToList();

	public int CurrentUnitIndex;


	public override void _Ready()
	{
		// Assign All units to their respective team
		PlayerCharacters = PlayerTeam.GetChildren().Cast<Character>().ToList();
		EnemyCharacters = EnemyTeam.GetChildren().Cast<Character>().ToList();
	}

	public Character GetCurrentUnit()
	{
		Character activeUnit = TurnOrder[CurrentUnitIndex];
		return activeUnit;
	}

	public Character AdvanceToNextUnit()
	{
		CurrentUnitIndex++;

        if (CurrentUnitIndex >= TurnOrder.Count)
		{
			GenerateTurnOrder();
		}
        
		return GetCurrentUnit();
	}
	
	public void StartBattleSequence()
    {
        GenerateTurnOrder();  // only generates order, no emit
        StartBattle?.Invoke();
    }
	
	private void GenerateTurnOrder()
	{
		TurnOrder.Clear();
		
		// Sort by DEX (High to Low)
		TurnOrder = AllUnits.OrderByDescending(unit => unit.StatsComponent.GetStat(StatType.Dex)).ToList();
		
		CurrentUnitIndex = 0;
		GD.Print($"Turn Order Generated. Next up: {TurnOrder[0].Name}");
	}
}

