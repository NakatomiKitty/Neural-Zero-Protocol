using Godot;
using Godot.Collections;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Combat;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
	/// <summary>
	/// Character.CS handles a Character's 7 Stats (HP, ATK, DEF, DEX, INT, LCK, NRG) and a Character's Rarity.
	/// Then applies the Rarity's Multiplier on 5 of the Stats (HP, ATK, DEF, DEX, INT)
	/// LCK is constant and NRG's value (5 or 10) is dependent on the Character's Rarity Tier
	/// Then stores the current values in a dictionary
	/// It also acts as the main hub for the components
	/// </summary>
    [GlobalClass]
	[Scene]
    public partial class Character : Node2D
    {
        [Node] public StatsComponent StatsComponent;
        [Node] public DisabilityComponent DisabilityComponent;
        [Node] public HealthComponent HealthComponent;
		[Node] public MovesetComponent MovesetComponent;

        [Export] private CharacterStatResource _characterStatsResources;

		public Dictionary<StatType, int> CurrentStats = new Dictionary<StatType, int>();

		public ElementType PrimaryElement => _characterStatsResources.PrimaryElement;
		public ElementType SecondaryElement => _characterStatsResources.SecondaryElement;


        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
			{
				WireNodes();
			}
        }

		public override void _EnterTree()
		{
			GD.Print(PrimaryElement);
			GD.Print(SecondaryElement);
			InitializeStats(_characterStatsResources);
		} 

		public override void _Ready() 
		{
			if (StatsComponent == null) GD.PushError("StatsComponent not found");

			if (_characterStatsResources == null)
			{
				GD.PushWarning("CharacterStatsResources not loaded lmao");
			}

			int maxHP = StatsComponent.GetStat(StatType.Hp);

			HealthComponent.InitializeHealth(maxHP);

			StatsComponent.StatZeroed += DisabilityComponent.OnStatZeroed;
			StatsComponent.StatRecovered += DisabilityComponent.OnStatRecovered;
		}
		
		
		// Initializes Stats before anything else
        private void InitializeStats(CharacterStatResource resource)
		{
			CurrentStats.Clear(); // clear just in case of re-initialization
			float mult = resource.GetStatMultiplier(resource.SelectedRarity); // Get the Rarity Multipliers
			
			foreach (StatType stat in Enum.GetValues<StatType>()) // loop through every StatType Values
			{
				// Get the Base Values from the resource
				int baseValue = resource.GetBaseValues(stat); 

				if (stat == StatType.Lck || stat == StatType.Nrg) // Makes Lck's value constant (2) AND makes Nrg either 5 or 10 depending on it's tier of rarity
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

    }
}
