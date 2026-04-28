using Godot;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using System;

namespace NeuralZeroProtocol.Scripts.Combat
{

    /// <summary>
    /// Handles all combat math: Evasion, Damage, CritDamage, CritChance, and Blocking
    /// All methods are static, this means THIS IS STRICTLY A MATH FUNCTION CLASS!
    /// </summary>
    
    public partial class DamageCalculator
    {
        private static Random _random = new Random();

        // if Atk > Dex + Lck, dodge the attack
        public static bool EvasionCheck(Character attacker, Character defender) => GetStatValue(defender, StatTypes.Dex) + (GetStatValue(defender, StatTypes.Lck) * 1.25f) < GetStatValue(attacker, StatTypes.Atk);

        public static int CalculateDamage(Character attacker, Character defender, MoveResource move)
        {
            if (move == null)
            {
                GD.PushWarning("No move selected lmao");
            }

            // if CalculateEvasionCheck is true, miss the attack
            if (EvasionCheck(attacker, defender)) return 0;

            int baseDamage = move.Power + GetStatValue(attacker, StatTypes.Atk) - (int)(GetStatValue(defender, StatTypes.Def) * 0.75f);

            // if it's a CriticalChance is true, apply Critical hit bonus.
            if(CriticalChance(attacker, defender, move))
            {
                int critBonus = (int)(GetStatValue(attacker, StatTypes.Atk) * (GetStatValue(attacker, StatTypes.Int) / 100.0f));
                return baseDamage + critBonus;
            }

            return baseDamage;
        }

        public static int CalculateAbsoluteBlock(Character attacker, Character defender, MoveResource move)
        {
            // TODO: This is for Shield Gauge
            float totalAttack = move.Power + GetStatValue(attacker, StatTypes.Atk) - GetStatValue(defender, StatTypes.Def);

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

            float critChance = GetStatValue(attacker, StatTypes.Lck) * 1.25f;
            critChance = Math.Clamp(critChance, 0, 100); //Clamp so the percentage doesn't go below zero or past 100

            int roll = _random.Next(0,100);
            
            return roll < critChance;
        }

        // Private Helper Functions

        private static bool IsDefenderBroken(Character defender) => defender.DisabilityComponent.HasDisability(DisabilityTypes.Broken);
        private static int GetStatValue(Character character, StatTypes stat) => character.StatsComponent.GetStat(stat);
    }

    
}

