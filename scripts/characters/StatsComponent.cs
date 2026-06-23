using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Characters;

/// <summary>
/// StatsComponent mostly handles getting the Stat values from Character.CS
/// And modifying them then emits signals when stats (except HP) hit 0 (DeadZone) or recover above 0.
/// </summary>

[GlobalClass]
public partial class StatsComponent : Node
{
	// TODO: THIS IS FOR CHECKING STATS INGAME ↓
	public event Action<int, int> StatChanged; // int stat, int currentValue 
	public event Action<int> StatZeroed; // int stat
	public event Action<int> StatRecovered; 

	private Character _character;

	public void Initialize(Character character) => _character = character;
	
	public int GetStat(StatType stat)
	{
		return _character.CurrentStats.GetValueOrDefault(stat, 5);
	}

	public void ModifyStat(StatType stat, int changeValue)
	{
		
		int oldValue = GetStat(stat);
		int currentValue = Math.Max(0, oldValue + changeValue);
		
		GD.Print($"Stat changed: {stat} ({oldValue} → {currentValue}) [changeValue: {changeValue}]");

		_character.CurrentStats[stat] = currentValue;
		
		StatChanged?.Invoke((int)stat, currentValue);

		// Enter DeadZone
		if (stat != StatType.Hp && oldValue > 0 && currentValue == 0) 
		{   
			GD.Print($"{stat} reached zero! Activating disability!");
			StatZeroed?.Invoke((int)stat);
		}
		
		// Leave DeadZone
		else if (oldValue == 0 && currentValue > 0) 
		{
			GD.Print($"{stat} recovered! Deactivating disability!");
			StatRecovered?.Invoke((int)stat);
		}
	}

	public void DebugPrintAllStats()
	{   
		GD.Print($"{GetParent().Name} stats");
		foreach (StatType stat in Enum.GetValues<StatType>()) // loop through every StatType Values
		{
			GD.Print($"{stat}: {GetStat(stat)}");
		}
	}
}

