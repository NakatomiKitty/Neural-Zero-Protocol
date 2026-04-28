using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class Character : Node2D
    {
        public StatsComponent StatsComponent;
        public DisabilityComponent DisabilityComponent;
        public HealthComponent HealthComponent;

        [Export] private CharacterStatResource _characterStatsResources;

		public Dictionary<StatTypes, int> CurrentStats = new Dictionary<StatTypes, int>();

		public override void _Ready() 
		{
			StatsComponent = GetNode<StatsComponent>("StatsComponent");
            DisabilityComponent = GetNode<DisabilityComponent>("DisabilityComponent");
            HealthComponent = GetNode<HealthComponent>("HealthComponent");

			if (StatsComponent == null) GD.PushError("StatsComponent not found");

			if (_characterStatsResources == null)
			{
				GD.PushWarning("CharacterStatsResources not loaded lmao");
			}

			int maxHP = StatsComponent.GetStat(StatTypes.Hp);

			HealthComponent.InitializeHealth(maxHP);

			StatsComponent.StatZeroed += DisabilityComponent.OnStatZeroed;
			StatsComponent.StatRecovered += DisabilityComponent.OnStatRecovered;
		}
		
		// Initializes Stats before anything else
		public override void _EnterTree() => InitializeStats(_characterStatsResources);

        private void InitializeStats(CharacterStatResource resource)
		{
			CurrentStats.Clear(); // clear just in case of re-initialization
			float mult = resource.GetStatMultiplier(resource.SelectedRarity); // Get the Rarity Multipliers
			
			foreach (StatTypes stat in Enum.GetValues<StatTypes>()) // loop through every StatType Values
			{
				// Get the Base Values from the resource
				int baseValue = resource.GetBaseValues(stat); 

				if (stat == StatTypes.Lck || stat == StatTypes.Nrg) // Makes Lck's value constant (2) AND makes Nrg either 5 or 10 depending on it's tier of rarity
				{
					if (stat == StatTypes.Nrg && resource.IsHighTierRarity())
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
