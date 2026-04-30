using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using Godot.Collections;

namespace NeuralZeroProtocol.Scripts.Characters
{
	public partial class TurnManager : Node
	{
		[Export] public Node PlayerTeam;
		[Export] public Node EnemyTeam;
		[Export] public Timer TurnTimer;

		public List<Character> TurnOrder = new List<Character>();

		public List<Character> PlayerCharacters;
		public List<Character> EnemyCharacters;
		public List<Character> AllUnits
		{
			get
			{
				return PlayerCharacters.Concat(EnemyCharacters).ToList();
			}
		}

		public int CurrentUnitIndex = 0;


		public override void _Ready()
		{
			// Combine all units from the containers
			PlayerCharacters = PlayerTeam.GetChildren().Cast<Character>().ToList();
			EnemyCharacters = EnemyTeam.GetChildren().Cast<Character>().ToList();

			// This ensures the SDK is working. Check your Output tab!
			GD.Print("TurnManager Base System: ONLINE.");
		}

		// Call this to start
		public void StartBattle()
		{
			GenerateTurnOrder();
		}

		public void GenerateTurnOrder()
		{
			TurnOrder.Clear();
			
			// Sort by DEX (High to Low). -
			TurnOrder = AllUnits.OrderByDescending(unit => unit.StatsComponent.GetStat(StatType.Dex)).ToList();
			
			CurrentUnitIndex = 0;
			GD.Print($"Turn Order Generated. Next up: {TurnOrder[0].Name}");
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
	}
}
