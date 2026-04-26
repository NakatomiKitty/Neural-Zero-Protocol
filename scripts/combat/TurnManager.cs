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

		private Array<Node> playerCharacters;
		private Array<Node> enemyCharacters;
		public List<Character> AllUnits;

		public int CurrentUnitIndex = 0;


		public override void _Ready()
		{
			// Combine all units from the containers
			playerCharacters = PlayerTeam.GetChildren();
			enemyCharacters = EnemyTeam.GetChildren();
			AllUnits = playerCharacters.Concat(enemyCharacters).Cast<Character>().ToList();

			// This ensures the SDK is working. Check your Output tab!
			GD.Print("TurnManager Base System: ONLINE.");
		}

		// Call this to start
		public void StartBattle()
		{
			GenerateTurnOrder();
		}

		private void GenerateTurnOrder()
		{
			TurnOrder.Clear();
			
			// Sort by DEX (High to Low). -
			TurnOrder = AllUnits.OrderByDescending(unit => unit.StatsComponent.GetStat(StatTypes.Dex)).ToList();
			
			CurrentUnitIndex = 0;
			GD.Print($"Turn Order Generated. First up: {TurnOrder[0].Name}");
		}

		public Character GetCurrentUnit()
		{
			Character activeUnit = TurnOrder[CurrentUnitIndex];
			return activeUnit;
		}
	}
}
