using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Characters
{

	// DisabilityComponent handles the Disability logic after a stat reached it's DeadZone
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
		private HashSet<DisabilityTypes> _currentDisabilities = new HashSet<DisabilityTypes>();

        // Links StatTypes to DisabilityTypes
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


		public bool HasDisability(DisabilityTypes disability) => _currentDisabilities.Contains(disability);

		public void OnStatZeroed(int statInt)
		{
			var disability = ToDisability((StatTypes)statInt);
			GD.Print($"{disability} ACTIVATED!");
			_currentDisabilities.Add(disability);
		}

		public void OnStatRecovered(int statInt)
		{
			var disability = ToDisability((StatTypes)statInt);
			GD.Print($"{disability} Deactivated!");
			_currentDisabilities.Remove(disability);
		}
		
		
		
	}
}
