using Godot;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using System;

namespace NeuralZeroProtocol.Autoloads;

/// <summary>
/// Handles all combat math: Evasion, Damage, CritDamage, CritChance, and Blocking
/// All methods are static, this means THIS IS STRICTLY A MATH FUNCTION CLASS!
/// </summary>

// TODO: REMOVE FROM AUTOLOAD AND MAKE THIS A STATIC CLASS
public partial class DamageCalculator : Node
{
    private static readonly Random Random = new();
    
    private static readonly float[,] TypeChart = new float[9, 9]
    {
    //          FIRE  NATURE GROUND  WIND    ICE   WATER  LIGHT  DARK   NULL
    /*FIRE*/   {0.75f, 0.75f, 1.50f, 2.00f, 0.75f, 1.50f, 0.75f, 1.50f, 1.00f},
    /*NATURE*/ {1.50f, 0.75f, 0.75f, 1.50f, 2.00f, 0.75f, 0.00f, 1.50f, 1.00f},
    /*GROUND*/ {0.75f, 1.50f, 1.00f, 0.00f, 1.50f, 2.00f, 1.00f, 1.00f, 1.00f},
    /*WIND*/   {2.00f, 0.75f, 0.00f, 1.00f, 0.75f, 1.50f, 1.00f, 1.00f, 1.00f},
    /*ICE*/    {1.50f, 2.00f, 0.75f, 0.75f, 0.75f, 0.75f, 1.50f, 0.75f, 1.00f},
    /*WATER*/  {0.75f, 1.50f, 2.00f, 1.50f, 1.50f, 0.75f, 1.50f, 0.75f, 1.00f},
    /*LIGHT*/  {0.75f, 1.50f, 1.00f, 1.00f, 0.75f, 0.75f, 0.75f, 2.00f, 1.00f},
    /*DARK*/   {0.75f, 0.75f, 1.00f, 1.00f, 1.50f, 1.50f, 2.00f, 0.75f, 1.00f},
    /*NULL*/   {1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f},
    };
    
    public static (bool dodged, bool immuned, bool isCrit, int damage) CalculateDamage(Character attacker, Character defender, MoveResource move)
    {
        if (move == null)
        {
            GD.PushWarning("No move selected lmao");
        }
        
        bool defenderDodged =  DefenderEvasionCheck(attacker, defender);
        if (defenderDodged) return (true, false, false, 0);
        
        ElementType attackElem = move.MoveElement; // move's element
        ElementType defPrimary = defender.PrimaryElement;
        ElementType defSecondary = defender.SecondaryElement;

        float typeMult = GetCombinedTypeMultiplier(attackElem, defPrimary, defSecondary);

        // Immunity check (0.0 multiplier)
        if (typeMult == 0.0f) return (false, true, false, 0);

        int baseDamage = move.Power + GetStatValue(attacker, StatType.Atk) - (int)(GetStatValue(defender, StatType.Def) * 0.5f);
        
        if (baseDamage < 0) baseDamage = 0;
        
        float damage = baseDamage * typeMult;
        // if it's a CriticalChance is true, apply Critical hit bonus.
        bool isCrit = CriticalChance(attacker, defender, move);
        if (isCrit)
        {
            float critBonus = (1.5f + GetStatValue(attacker, StatType.Int) / 100.0f);
            damage *= critBonus;
        }

        return (false, false, isCrit, (int)damage);
    }
    
    // if attackerAtk < defenderDex + defenderLck, defender will dodge the attack
    private static bool DefenderEvasionCheck(Character attacker, Character defender)
    {
        return GetStatValue(attacker, StatType.Atk) <
               GetStatValue(defender, StatType.Dex) + GetStatValue(defender, StatType.Lck) * 1.25f;
    }
        

    private static int CalculateAbsoluteBlock(Character attacker, Character defender, MoveResource move)
    {
        // TODO: This is for Shield Gauge
        float totalAttack = move.Power + GetStatValue(attacker, StatType.Atk) - GetStatValue(defender, StatType.Def);

        return (int)totalAttack;
    }

    private static bool CriticalChance(Character attacker, Character defender, MoveResource move)
    {
        const float minCritChance = 0f;
        const float maxCritChance = 100f;
        if (IsDefenderBroken(defender)) return true;
        
        if (move == null || !move.CanCrit) return false;
        
        if (attacker.DisabilityComponent.HasDisability(DisabilityTypes.Cursed)) return false;
        
        float critChance = GetStatValue(attacker, StatType.Lck) * 1.25f;
        
        critChance = Math.Clamp(critChance, minCritChance, maxCritChance); 
        
        int roll = Random.Next(0,100);
        
        return roll < critChance;
    }

    // Private Helper Functions

    private static bool IsDefenderBroken(Character defender) => defender.DisabilityComponent.HasDisability(DisabilityTypes.Broken);
    private static int GetStatValue(Character character, StatType stat) => character.StatsComponent.GetStat(stat);

    // Get mult (for all characters)
    private static float GetTypeMultiplier(ElementType move, ElementType defend)
    {
        if (move == ElementType.None || defend == ElementType.None)
            return 1.0f;
        
        return TypeChart[(int)defend, (int)move];
    }

    // Get combined mult (for dual type characters)
    private static float GetCombinedTypeMultiplier(ElementType moveElement, ElementType defendPrimary, ElementType defendSecondary)
    {
        float mult = GetTypeMultiplier(moveElement, defendPrimary);
        
        if (defendSecondary != ElementType.None && defendSecondary != defendPrimary)
            mult *= GetTypeMultiplier(moveElement, defendSecondary);
        
        return mult;
    }
}


