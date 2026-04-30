using Godot;
using NeuralZeroProtocol.Scripts.Combat;
using System;
using System.Collections.Generic;
namespace NeuralZeroProtocol.Scripts.Resources.CharacterData
{   
    public enum StatType
    {
        Hp,
        Atk,
        Def,
        Dex,
        Int,
        Lck,
        Nrg,
    }
    
    [GlobalClass]
    public partial class CharacterStatResource : Resource
    {
        private static readonly Dictionary<Rarity, float> _fibonacciMultiplier = new Dictionary<Rarity, float>()
        {
            { Rarity.Scrap, 1.0f },
            { Rarity.Common, 1.15f },
            { Rarity.Uncommon, 1.25f },
            { Rarity.Rare, 1.25f },
            { Rarity.SuperRare, 1.50f },
            { Rarity.UltraRare, 1.50f },
        };

        [Export] public ElementType CharacterElement01 = ElementType.None;
        [Export] public ElementType CharacterElement02 = ElementType.None;
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
            float baseMultiplier = 1.0f; // named scrapMultiplier to show that this really is a fibonacci sequence or some shit idk

            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                baseMultiplier *= _fibonacciMultiplier[rarity]; // think of this like a pastMultiplier * newMultiplier.
                
                if (rarity == rarityName) break;
            }
            return baseMultiplier;
        }

        public bool IsHighTierRarity() => SelectedRarity >= Rarity.Rare;

        public int GetBaseValues(StatType stat)
        {
            switch (stat)
            {
                case StatType.Hp:
                    return MaxHealth;
                case StatType.Atk:
                    return AtkBase;
                case StatType.Def:
                    return DefBase;
                case StatType.Dex:
                    return DexBase;
                case StatType.Int:
                    return IntBase;
                case StatType.Lck:
                    return LckBase;
                case StatType.Nrg:
                    return NrgBase;
                default:
                    return 0;
            }
        }
    }
}

