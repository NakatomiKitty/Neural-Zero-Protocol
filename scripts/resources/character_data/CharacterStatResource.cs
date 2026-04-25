using Godot;
using System;
using System.Collections.Generic;
namespace NeuralZeroProtocol.Scripts.Resources.CharacterData
{   
    public enum StatTypes
    {
        Hp,
        Atk,
        Def,
        Dex,
        Int,
        Lck,
        Nrg,
    }

    public enum Rarity
    {
        Scrap,
        Common,
        Uncommon,
        Rare,
        SuperRare,
        UltraRare,
    }
    
    [GlobalClass]
    public partial class CharacterStatResource : Resource
    {
        private static readonly Dictionary<Rarity, float> _fibonacciMultiplier = new Dictionary<Rarity, float>()
        {
            { Rarity.Common, 1.15f },
            { Rarity.Uncommon, 1.25f },
            { Rarity.Rare, 1.25f },
            { Rarity.SuperRare, 1.50f },
            { Rarity.UltraRare, 1.50f },
        };

        [Export] public Rarity SelectedRarity = Rarity.Scrap;

        [Export] public int MaxHealth = 500;
        [Export] private int AtkBase = 20;
        [Export] private int DefBase = 20;
        [Export] private int DexBase = 10;
        [Export] private int IntBase = 10;
        [Export] private int LckBase = 1;
        [Export] private int NrgBase = 5;

        public float GetStatMultiplier(Rarity rarityName)
        {
            float scrapMultiplier = 1.0f; // named scrapMultiplier to show that this really is a fibonacci sequence or some shit idk

            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                if (rarity == Rarity.Scrap) continue; // Scrap has no set factor, so this will immediately return the scrapMultiplier for it's value instead
                scrapMultiplier *= _fibonacciMultiplier[rarity]; // think of this like a pastMultiplier * newMultiplier.
                if (rarity == rarityName) break;
            }
            return scrapMultiplier;
        }

        public bool IsHighTierRarity() => SelectedRarity >= Rarity.Rare;

        public int GetBaseValues(StatTypes stat)
        {
            switch (stat)
            {
                case StatTypes.Hp:
                    return MaxHealth;
                case StatTypes.Atk:
                    return AtkBase;
                case StatTypes.Def:
                    return DefBase;
                case StatTypes.Dex:
                    return DexBase;
                case StatTypes.Int:
                    return IntBase;
                case StatTypes.Lck:
                    return LckBase;
                case StatTypes.Nrg:
                    return NrgBase;
                default:
                    return 0;
            }
        }
    }
}

