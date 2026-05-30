using System;
using System.Collections.Generic;
using Godot;
using NeuralZeroProtocol.Autoloads;

namespace NeuralZeroProtocol.Scripts.Resources.CharacterData;

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
    private static readonly Dictionary<Rarity, float> FibonacciMultiplier = new Dictionary<Rarity, float>()
    {
        { Rarity.Scrap, 1.0f },
        { Rarity.Common, 1.15f },
        { Rarity.Uncommon, 1.25f },
        { Rarity.Rare, 1.25f },
        { Rarity.SuperRare, 1.50f },
        { Rarity.UltraRare, 1.50f },
    };

    [Export] public ElementType PrimaryElement = ElementType.None;
    [Export] public ElementType SecondaryElement = ElementType.None;
    [Export] public Rarity SelectedRarity = Rarity.Scrap;

    [Export] public int MaxHealth = 500;
    [Export] private int _atkBase = 20;
    [Export] private int _defBase = 20;
    [Export] private int _dexBase = 10;
    [Export] private int _intBase = 10;
    [Export] private int _lckBase = 1;
    [Export] private int _nrgBase = 5;

    public float GetStatMultiplier(Rarity rarityName)
    {
        float baseMultiplier = 1.0f; // named scrapMultiplier to show that this really is a fibonacci sequence or some shit idk

        foreach (Rarity rarity in Enum.GetValues<Rarity>())
        {
            baseMultiplier *= FibonacciMultiplier[rarity]; // think of this like a pastMultiplier * newMultiplier.
            
            if (rarity == rarityName) break;
        }
        return baseMultiplier;
    }

    public bool IsHighTierRarity() => SelectedRarity >= Rarity.Rare;

    public int GetBaseValues(StatType stat)
    {
        return stat switch
        {
            StatType.Hp => MaxHealth,
            StatType.Atk => _atkBase,
            StatType.Def => _defBase,
            StatType.Dex => _dexBase,
            StatType.Int => _intBase,
            StatType.Lck => _lckBase,
            StatType.Nrg => _nrgBase,
            _ => 0
        };
    }
}

