using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;

namespace NeuralZeroProtocol.Scripts.Characters
{
	public partial class TurnManager : Node
	{
		[Export] public Node PlayerPosition;
		[Export] public Node EnemyPosition;
		[Export] public Timer TurnTimer;

		private List<Node> _turnOrder = new List<Node>();
		private int _currentUnitIndex = 0;

		public override void _Ready()
		{
			// This ensures the SDK is working. Check your Output tab!
			GD.Print("TurnManager Base System: ONLINE.");
		}

		// Call this to start
		public void StartBattle()
		{
			GD.Print("Battle Initializing...");
			GenerateTurnOrder();
			ExecuteTurn();
		}

		private void GenerateTurnOrder()
		{
			_turnOrder.Clear();
			
			// Combine all units from the containers
			var players = PlayerPosition.GetChildren().Cast<Node>();
			var enemies = EnemyPosition.GetChildren().Cast<Node>();
			var allUnits = players.Concat(enemies).ToList();

			// Sort by DEX (High to Low). 
			_turnOrder = allUnits.OrderByDescending(u => u.Name).ToList(); // Temporary sort by name to test
			
			_currentUnitIndex = 0;
			GD.Print($"Turn Order Generated. First up: {_turnOrder[0].Name}");
		}

		private async void ExecuteTurn()
		{
			if (_currentUnitIndex >= _turnOrder.Count)
			{
				GD.Print("Round Over. Refreshing...");
				GenerateTurnOrder();
				return;
			}

			Node activeUnit = _turnOrder[_currentUnitIndex];
			
			// --- Friend code here for the execution part lol ---
			
			TurnTimer.Start();
			await ToSignal(TurnTimer, "timeout");

			GD.Print($"{activeUnit.Name} finished their turn.");
			_currentUnitIndex++;
			ExecuteTurn();
		}
	}
}
