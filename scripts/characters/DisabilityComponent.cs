using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Characters
{
	public enum DisabilityTypes
	{
		Fragile,
		Broken,
		Stiff,
		Mindless,
		Cursed,
		Exhausted,
	}

	[GlobalClass]
	public partial class DisabilityComponent : Node
	{
		private StatsComponent _statsComponent;

		private HashSet<DisabilityTypes> _currentDisabilities = new HashSet<DisabilityTypes>();

		public override void _Ready() 
		{
			_statsComponent = GetNode<StatsComponent>("../StatsComponent");

			// Check if null
			if (_statsComponent == null)
			{
				GD.PushWarning("StatsComponent not loaded lmao");
			}

			_statsComponent.StatZeroed += OnStatZeroed;
			_statsComponent.StatRecovered += OnStatRecovered;
		}

		private DisabilityTypes ToDisability(StatTypes statTypes) => statTypes switch
		{
			StatTypes.Atk => DisabilityTypes.Fragile,
			StatTypes.Def => DisabilityTypes.Broken,
			StatTypes.Dex => DisabilityTypes.Stiff,
			StatTypes.Int => DisabilityTypes.Mindless,
			StatTypes.Lck => DisabilityTypes.Cursed,
			StatTypes.Nrg => DisabilityTypes.Exhausted,
			_ => 0,
		};


		private void OnStatZeroed(int statInt)
		{
			var disability = ToDisability((StatTypes)statInt);
			GD.Print($"{disability} ACTIVATED!");
			_currentDisabilities.Add(disability);
		}

		private void OnStatRecovered(int statInt)
		{
			var disability = ToDisability((StatTypes)statInt);
			GD.Print($"{disability} Deactivated!");
			_currentDisabilities.Remove(disability);
		}
		
		public bool HasDisability(DisabilityTypes disability)
		{
			return _currentDisabilities.Contains(disability);
		}
		
	}
}
