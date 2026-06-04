using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Autoloads;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;


namespace NeuralZeroProtocol.Scripts.Characters;

/// <summary>
/// Character.CS handles a Character's 7 Stats (HP, ATK, DEF, DEX, INT, LCK, NRG) and a Character's Rarity.
/// Then applies the Rarity's Multiplier on 5 of the Stats (HP, ATK, DEF, DEX, INT)
/// LCK is constant and NRG's value (5 or 10) is dependent on the Character's Rarity Tier
/// Then stores the current values in a dictionary
/// It also acts as the main hub for the components
/// </summary>
[GlobalClass]
public partial class Character : Node2D
{
    public StatsComponent StatsComponent;
    public DisabilityComponent DisabilityComponent;
    public HealthComponent HealthComponent;
	public MovesetComponent MovesetComponent;

    [Export] private CharacterStatResource _characterStatsResources;

	public Dictionary<StatType, int> CurrentStats = new();

	public ElementType PrimaryElement => _characterStatsResources.PrimaryElement;
	public ElementType SecondaryElement => _characterStatsResources.SecondaryElement;
	
	public override void _EnterTree()
	{
		GD.Print(PrimaryElement);
		GD.Print(SecondaryElement);
		Initialize();
		InitializeStats(_characterStatsResources);
	} 

	public override void _Ready() 
	{
		int maxHp = StatsComponent.GetStat(StatType.Hp);

		HealthComponent.InitializeHealth(maxHp);

		StatsComponent.StatZeroed += DisabilityComponent.OnStatZeroed;
		StatsComponent.StatRecovered += DisabilityComponent.OnStatRecovered;
	}

	private void Initialize()
	{
		StatsComponent = GetNode<StatsComponent>("StatsComponent");
		DisabilityComponent = GetNode<DisabilityComponent>("DisabilityComponent");
		HealthComponent = GetNode<HealthComponent>("HealthComponent");
		MovesetComponent = GetNode<MovesetComponent>("MovesetComponent");
	}
	
    private void InitializeStats(CharacterStatResource resource)
	{
		CurrentStats.Clear();
		
		float mult = resource.GetStatMultiplier(resource.SelectedRarity); 
		
		foreach (StatType stat in Enum.GetValues<StatType>())
		{
			int baseValue = resource.GetBaseValues(stat); 
			
			// Makes Lck's value constant (2) AND makes Nrg either 5 or 10 depending on it's tier of rarity
			if (stat is StatType.Lck or StatType.Nrg) 
			{
				if (stat == StatType.Nrg && resource.IsHighTierRarity())
				{
					CurrentStats[stat] = 10;
					continue;
				}

				CurrentStats[stat] = baseValue;
				continue;
			}
			
			float finalValue = MathF.Ceiling(baseValue * mult); // Applies the Rarity Multiplier
			CurrentStats[stat] = (int)finalValue;
		}

		// Debug Print
		GD.Print(resource.SelectedRarity);
	}

	public override void _ExitTree()
	{
		StatsComponent.StatZeroed -= DisabilityComponent.OnStatZeroed;
		StatsComponent.StatRecovered -= DisabilityComponent.OnStatRecovered;
	}
}

