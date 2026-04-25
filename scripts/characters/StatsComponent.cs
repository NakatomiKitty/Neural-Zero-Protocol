using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;
using System.Collections.Generic;



namespace NeuralZeroProtocol.Scripts.Characters
{
	/// <summary>
	/// StatsComponent handles a Character's 7 Stats (HP, ATK, DEF, DEX, INT, LCK, NRG) and a Character's Rarity.
	/// Then applies the Rarity's Multiplier on 5 of the Stats (HP, ATK, DEF, DEX, INT)
	/// LCK is constant and NRG's value (5 or 10) is dependent on the Character's Rarity Tier
	/// It stores current values in a dictionary then emits signals when stats (except HP) hit 0 (DeadZone) or recover above 0.
	/// </summary>

	[GlobalClass]
	public partial class StatsComponent : Node
	{
		// Declaration of Signals
		[Signal] public delegate void StatChangedEventHandler(int stat, int currentValue);
		[Signal] public delegate void StatZeroedEventHandler(int stat);
		[Signal] public delegate void StatRecoveredEventHandler(int stat);

		[Export] private CharacterStatResource _characterStatsResources;

		private Dictionary<StatTypes, int> _currentStats = new Dictionary<StatTypes, int>();

		public override void _Ready() {
			// If you forget to assign a CharacterStatsResources in the editor, the game would crash when a character uses this component.
			// Adding this here will atleast notifies us early :)
			if (_characterStatsResources == null)
			{
				GD.PushWarning("CharacterStatsResources not loaded lmao");
			}

			InitializeFromResource(_characterStatsResources);
			DebugPrintAllStats();

            // if you want to debug, put ModifyStat(StatTypes.Key, value)

		}

		private void InitializeFromResource(CharacterStatResource resource)
		{
			_currentStats.Clear(); // clear just in case of re-initialization
			float mult = resource.GetStatMultiplier(resource.SelectedRarity); // Get the Rarity Multipliers
			
			foreach (StatTypes stat in Enum.GetValues<StatTypes>()) // loop through every StatType Values
			{
				// Get the Base Values from the resource
				int baseValue = resource.GetBaseValues(stat); 

				if (stat == StatTypes.Lck || stat == StatTypes.Nrg) // Makes Lck's value constant (2) AND makes Nrg either 5 or 10 depending on it's tier of rarity
				{
					if (stat == StatTypes.Nrg && resource.IsHighTierRarity())
					{
						_currentStats[stat] = 10;
						continue;
					}

					_currentStats[stat] = baseValue;
					continue;
				}
				
				float finalValue = MathF.Ceiling(baseValue * mult); // Applies the Rarity Multiplier
				_currentStats[stat] = (int)finalValue;
			}

			// Debug Print
			GD.Print(resource.GetStatMultiplier(resource.SelectedRarity));
			GD.Print($"Stats initialized for {Owner.Name}: ATK={GetStat(StatTypes.Atk)}, DEF={GetStat(StatTypes.Def)}, NRG={GetStat(StatTypes.Nrg)}");
		}

		public int GetStat(StatTypes stat) // this retrieves the old value
		{
			if (_currentStats.TryGetValue(stat, out int value))
			{
				return value;
			}
			return 0;
		}

		public void ModifyStat(StatTypes stat, int changeValue)
		{

			var oldValue = GetStat(stat);
			var currentValue = Math.Max(0, oldValue + changeValue); // Clamp to prevent going past below zero

			// Debug print
			GD.Print($"Stat changed: {stat} ({oldValue} → {currentValue}) [changeValue: {changeValue}]");

			_currentStats[stat] = currentValue;

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
			GD.Print($"{Owner.Name} stats");
			foreach (StatTypes stat in Enum.GetValues<StatTypes>()) // loop through every StatType Values
			{
				if (stat == StatTypes.Lck)
				{
					GD.Print($"{stat}: {_currentStats[StatTypes.Lck] * 1.25f}");
					continue;
				}
				GD.Print($"{stat}: {GetStat(stat)}");
			}
		}
	}
}
