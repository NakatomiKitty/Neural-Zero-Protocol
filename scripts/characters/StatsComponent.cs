using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;
using System.Collections.Generic;



namespace NeuralZeroProtocol.Scripts.Characters
{
	/// <summary>
	/// StatsComponent mostly handles getting the Stat values from Character.CS
	/// And modifying them then emits signals when stats (except HP) hit 0 (DeadZone) or recover above 0.
	/// </summary>

	[GlobalClass]
	public partial class StatsComponent : Node
	{
		// Declaration of Signals
		[Signal] public delegate void StatChangedEventHandler(int stat, int currentValue);
		[Signal] public delegate void StatZeroedEventHandler(int stat);
		[Signal] public delegate void StatRecoveredEventHandler(int stat);

		private Character _character;

		public override void _Ready() 
		{
			if (_character == null)
			{
				GD.PushWarning($"Character is not loaded in!");
			}
			
			DebugPrintAllStats();
            // if you want to debug, put ModifyStat(StatTypes.Key, value)
		}

		public void Initialize(Character character) => _character = character;
		
		public int GetStat(StatTypes stat) // this retrieves the old value
		{
			if (_character.CurrentStats.TryGetValue(stat, out int value))
			{
				return value;
			}
			return 5;
		}

		public void ModifyStat(StatTypes stat, int changeValue)
		{

			var oldValue = GetStat(stat);
			var currentValue = Math.Max(0, oldValue + changeValue); // Clamp to prevent going past below zero

			// Debug print
			GD.Print($"Stat changed: {stat} ({oldValue} → {currentValue}) [changeValue: {changeValue}]");

			_character.CurrentStats[stat] = currentValue;

			// Tell any listening nodes that this stat's value has changed.
			// Example: If DEX goes from 5 to 1, we send: (2(stat), 1(newValue)) because DEX = 2 in the enum.
			EmitSignal(SignalName.StatChanged, (int)stat, currentValue);

			// If the stat was positive and now becomes exactly zero, it means the character just entered the "DeadZone".
			// Example: ATK drops from 5 to 0 → character becomes "Fragile". etc. etc. etc.
			
			if (stat != StatTypes.Hp && oldValue > 0 && currentValue == 0) 
			{   
				GD.Print($"{stat} reached zero! Activating disability!");
				EmitSignal(SignalName.StatZeroed, (int)stat);
			}

			// If the stat was zero and was recently recovered, it means the character has gotten out of the "DeadZone"
			// Character is no more fragile YIPPIEEE
			else if (oldValue == 0 && currentValue > 0) 
			{
				GD.Print($"{stat} recovered! Deactivating disability!");
				EmitSignal(SignalName.StatRecovered, (int)stat);
			}
		}

		public void DebugPrintAllStats()
		{   
			GD.Print($"{GetParent().Name} stats");
			foreach (StatTypes stat in Enum.GetValues<StatTypes>()) // loop through every StatType Values
			{
				GD.Print($"{stat}: {GetStat(stat)}");
			}
		}
	}
}
