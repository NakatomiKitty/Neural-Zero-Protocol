using Godot;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Combat
{

    /// <summary>
    /// Handles all combat math: Evasion, Damage, CritDamage, CritChance, and Blocking
    /// All methods are static, this means THIS IS STRICTLY A MATH FUNCTION CLASS!
    /// </summary>
    
    public partial class DamageCalculator
    {
        private static Random _random = new Random();
        
        private static readonly float[,] _typeChart = new float[9, 9]
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

        // if Atk > Dex + Lck, dodge the attack
        public static bool EvasionCheck(Character attacker, Character defender) => 
            GetStatValue(defender, StatType.Dex) + 
            (GetStatValue(defender, StatType.Lck) * 1.25f) < 
            GetStatValue(attacker, StatTyp e.Atk);

        
        public static int CalculateDamage(Character attacker, Character defender, MoveResource move)
        {
            if (move == null)
            {
                GD.PushWarning("No move selected lmao");
            }

            // if CalculateEvasionCheck is true, miss the attack
            if (EvasionCheck(attacker, defender)) return 0;

            ElementType attackElem = move.MoveElement; // move's element
            ElementType defPrimary = defender.PrimaryElement;
            ElementType defSecondary = defender.SecondaryElement;

            float typeMult = GetCombinedTypeMultiplier(attackElem, defPrimary, defSecondary);

            // Immunity check (0.0 multiplier)
            if (typeMult == 0.0f) return 0;

            int baseDamage = move.Power + GetStatValue(attacker, StatType.Atk) - ((int)(GetStatValue(defender, StatType.Def) * 0.5f));
            if (baseDamage < 0) baseDamage = 0;

            float damage = baseDamage * typeMult;

            // if it's a CriticalChance is true, apply Critical hit bonus.
            if(CriticalChance(attacker, defender, move))
            {
                int critBonus = (int)(1.5f + (GetStatValue(attacker, StatType.Int) / 100.0f));
                damage *= critBonus;
            }

            return (int)damage;
        }

        public static int CalculateAbsoluteBlock(Character attacker, Character defender, MoveResource move)
        {
            // TODO: This is for Shield Gauge
            float totalAttack = move.Power + GetStatValue(attacker, StatType.Atk) - GetStatValue(defender, StatType.Def);

            return (int)totalAttack;
        }

        public static bool CriticalChance(Character attacker, Character defender, MoveResource move)
        {
            // If defender has "broken", always return true
            if (IsDefenderBroken(defender)) return true;

            // if move cannot crit, return false
            if (move == null || !move.CanCrit) return false;
            
            // if attacker has "Cursed" disability, return false
            if (attacker.DisabilityComponent.HasDisability(DisabilityTypes.Cursed)) return false;

            float critChance = GetStatValue(attacker, StatType.Lck) * 1.25f;
            critChance = Math.Clamp(critChance, 0, 100); //Clamp so the percentage doesn't go below zero or past 100

            int roll = _random.Next(0,100);
            
            return roll < critChance;
        }

        // Private Helper Functions

        private static bool IsDefenderBroken(Character defender) => defender.DisabilityComponent.HasDisability(DisabilityTypes.Broken);
        private static int GetStatValue(Character character, StatType stat) => character.StatsComponent.GetStat(stat);

        // Get mult if this is a single type character
        private static float GetTypeMultiplier(ElementType move, ElementType defend)
        {
            if (move == ElementType.None || defend == ElementType.None)
                return 1.0f;
            
            return _typeChart[(int)move, (int)defend];
        }

        // Get mult if this is a single type character
        private static float GetCombinedTypeMultiplier(ElementType moveElement, ElementType defendPrimary, ElementType defendSecondary)
        {
            float mult = GetTypeMultiplier(moveElement, defendPrimary);
            
            if (defendSecondary != ElementType.None && defendSecondary != defendPrimary)
                mult *= GetTypeMultiplier(moveElement, defendSecondary);
            
            return mult;
        }
    }
}

