using System;
using GdUnit4;
using NeuralZeroProtocol.Autoloads;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using static GdUnit4.Assertions;

namespace NeuralZeroProtocol.Tests.Autoloads;

[TestSuite]
[RequireGodotRuntime]
public class TestDamageCalculator
{
    private Character _attacker;
    private StatsComponent _attackerStatsComponent;
    private DisabilityComponent _attackerDisabilityComponent;

    private Character _defender;
    private StatsComponent _defenderStatsComponent;
    private DisabilityComponent _defenderDisabilityComponent;
    private CharacterStatResource _defenderStatResource;

    private MoveResource _move;
    [BeforeTest]
    public void Setup()
    {
        // Setup the Player Character (Attacker in this Case)
        _attacker = AutoFree(new PlayerCharacter());
        
        SetCharacterStatBase(_attacker, StatType.Hp, 500);
        SetCharacterStatBase(_attacker, StatType.Atk, 20);
        SetCharacterStatBase(_attacker, StatType.Def, 20);
        SetCharacterStatBase(_attacker, StatType.Dex, 10);
        SetCharacterStatBase(_attacker, StatType.Int, 10);
        SetCharacterStatBase(_attacker, StatType.Lck, 1);
        SetCharacterStatBase(_attacker, StatType.Nrg, 5);

        _attackerStatsComponent = AutoFree(new StatsComponent());
        _attackerDisabilityComponent = AutoFree(new DisabilityComponent());
        
        _attackerStatsComponent.Initialize(_attacker);
        _attacker.StatsComponent = _attackerStatsComponent;
        _attacker.DisabilityComponent = _attackerDisabilityComponent;
        
        _defenderStatResource= AutoFree(new CharacterStatResource());
        _defender = AutoFree(new PlayerCharacter());
        
        SetCharacterStatBase(_defender, StatType.Hp, 500);
        SetCharacterStatBase(_defender, StatType.Atk, 20);
        SetCharacterStatBase(_defender, StatType.Def, 20);
        SetCharacterStatBase(_defender, StatType.Dex, 10);
        SetCharacterStatBase(_defender, StatType.Int, 10);
        SetCharacterStatBase(_defender, StatType.Lck, 1);
        SetCharacterStatBase(_defender, StatType.Nrg, 5);

        _defenderStatsComponent = AutoFree(new StatsComponent());
        _defenderDisabilityComponent = AutoFree(new DisabilityComponent());

        _defenderStatsComponent.Initialize(_defender);
        _defender.StatsComponent = _defenderStatsComponent;
        _defender.DisabilityComponent = _defenderDisabilityComponent;
    }
    
    // Base Damage Tests (Singular Element)
    [TestCase(ElementType.Null, ElementType.Null, 35, TestName = "CalculateDamage_BaseSingle1xDamage_NullVsNull")]
    [TestCase(ElementType.Fire, ElementType.Nature, 26, TestName = "CalculateDamage_BaseSingle0.75xDamage_FireVsNature")]
    [TestCase(ElementType.Fire, ElementType.Water, 52, TestName = "CalculateDamage_BaseSingle1.5xDamage_FireVsWater")]
    [TestCase(ElementType.Fire, ElementType.Wind, 70, TestName = "CalculateDamage_BaseSingle2xDamage_FireVsWind")]
    public void CalculateDamage_TypeMultiplier(ElementType defenderElement, ElementType moveElement, int expectedDamage)
    {
        SetupDefenderElement(defenderElement);
        SetMoveStats(moveElement, 25, false);
        
        MakeDefenderStiff();
        
        (bool isDodged, bool isEnemyImmune, bool isCrit, int damageDealt) = 
            DamageCalculator.CalculateDamage(_attacker, _defender, _move);
        
        AssertThat(isDodged).IsFalse();
        AssertThat(isEnemyImmune).IsFalse();
        AssertThat(isCrit).IsFalse();
        AssertThat(damageDealt).IsEqual(expectedDamage);
    }
    
    [TestCase(ElementType.Nature, ElementType.Light, 0)]
    public void CalculateDamage_Immune(ElementType defenderElement, ElementType moveElement, int expectedDamage)
    {
        SetupDefenderElement(defenderElement);
        SetMoveStats(moveElement, 25, false);
        
        MakeDefenderStiff();

        (bool isDodged, bool isEnemyImmune, bool isCrit, int damageDealt) = 
            DamageCalculator.CalculateDamage(_attacker, _defender, _move);
        
        AssertThat(isDodged).IsFalse();
        AssertThat(isEnemyImmune).IsTrue();
        AssertThat(isCrit).IsFalse();
        AssertThat(damageDealt).IsEqual(expectedDamage);
    }
    
    // Base Damage Tests (Dual Element)
    [TestCase(ElementType.Fire, ElementType.Nature, ElementType.Nature, 19, TestName = "CalculateDamage_BaseDual0.5675xDamage_FireNatureVsNature")]
    [TestCase(ElementType.Fire, ElementType.Ground, ElementType.Nature, 39, TestName = "CalculateDamage_BaseDual1.125xDamage_FireGroundVsNature")]
    [TestCase(ElementType.Fire, ElementType.Ice, ElementType.Nature, 52, TestName = "CalculateDamage_BaseDual1.5xDamage_FireIceVsNature")]
    [TestCase(ElementType.Ice, ElementType.Nature, ElementType.Fire, 78, TestName = "CalculateDamage_BaseDual2.25xDamage_IceNatureVsFire")]
    [TestCase(ElementType.Ice, ElementType.Wind, ElementType.Fire, 105, TestName = "CalculateDamage_BaseDual3xDamage_IceWindVsFire")]
    public void CalculateDamage_DualTypeMultiplier(ElementType defenderElement1, ElementType defenderElement2, ElementType moveElement, int expectedDamage)
    {
        SetupDefenderElement(defenderElement1, defenderElement2);
        SetMoveStats(moveElement, 25, false);
        
        MakeDefenderStiff();
        
        (bool isDodged, bool isEnemyImmune, bool isCrit, int damageDealt) = 
            DamageCalculator.CalculateDamage(_attacker, _defender, _move);
        
        AssertThat(isDodged).IsFalse();
        AssertThat(isEnemyImmune).IsFalse();
        AssertThat(isCrit).IsFalse();
        AssertThat(damageDealt).IsEqual(expectedDamage);
    }
    
    
    // Critical Damage Tests (Single Element
    [TestCase(ElementType.Null, ElementType.Null, 72, TestName = "CalculateDamage_CritSingle1xDamage_NullVsNull")]
    [TestCase(ElementType.Fire, ElementType.Nature, 54, TestName = "CalculateDamage_CritSingle0.75xDamage_FireVsNature")]
    [TestCase(ElementType.Fire, ElementType.Water, 108, TestName = "CalculateDamage_CritSingle1.5xDamage_FireVsWater")]
    [TestCase(ElementType.Fire, ElementType.Wind, 144, TestName = "CalculateDamage_CritSingle2xDamage_FireVsWind")]
    public void CalculateDamage_TypeMultiplierWithCrit(ElementType defenderElement, ElementType moveElement, int expectedDamage)
    {
        SetupDefenderElement(defenderElement);
        SetMoveStats(moveElement, 25, true);
        
        // Give Defender 0 Def to guarantee Crit
        MakeDefenderBroken();
        MakeDefenderStiff();
        
        (bool isDodged, bool isEnemyImmune, bool isCrit, int damageDealt) = 
            DamageCalculator.CalculateDamage(_attacker, _defender, _move);
        
        AssertThat(isDodged).IsFalse();
        AssertThat(isEnemyImmune).IsFalse();
        AssertThat(isCrit).IsTrue();
        AssertThat(damageDealt).IsEqual(expectedDamage);
    }
    
    // Crit Damage Tests (Dual Element)
    [TestCase(ElementType.Fire, ElementType.Nature, ElementType.Nature, 40, TestName = "CalculateDamage_CritDual0.5675xDamage_FireNatureVsNature")]
    [TestCase(ElementType.Fire, ElementType.Ground, ElementType.Nature, 81, TestName = "CalculateDamage_CritDual1.125xDamage_FireGroundVsNature")]
    [TestCase(ElementType.Fire, ElementType.Ice, ElementType.Nature, 108, TestName = "CalculateDamage_CritDual1.5xDamage_FireIceVsNature")]
    [TestCase(ElementType.Ice, ElementType.Nature, ElementType.Fire, 162, TestName = "CalculateDamage_CritDual2.25xDamage_IceNatureVsFire")]
    [TestCase(ElementType.Ice, ElementType.Wind, ElementType.Fire, 216, TestName = "CalculateDamage_CritDual3xDamage_IceWindVsFire")]
    public void CalculateDamage_DualTypeMultiplierWithCrit(ElementType defenderElement1, ElementType defenderElement2, ElementType moveElement, int expectedDamage)
    {
        SetupDefenderElement(defenderElement1, defenderElement2);
        SetMoveStats(moveElement, 25, true);
        
        // Give Defender 0 Def to guarantee Crit
        MakeDefenderBroken();
        MakeDefenderStiff();
        
        (bool isDodged, bool isEnemyImmune, bool isCrit, int damageDealt) = 
            DamageCalculator.CalculateDamage(_attacker, _defender, _move);
        
        AssertThat(isDodged).IsFalse();
        AssertThat(isEnemyImmune).IsFalse();
        AssertThat(isCrit).IsTrue();
        AssertThat(damageDealt).IsEqual(expectedDamage);
    }
    
    // Dodging
    [TestCase(20, 20, 0.5f, TestName = "EqualStats_Gives0.50MissChance")] 
    [TestCase(20, 0, 0.05f, TestName = "DexZero_Gives0.05MissChance")] 
    [TestCase(0, 10, 0.70f, TestName = "AttackZero_Gives0.70%MissChance")] 
    [TestCase(100000, 100000, 0.50f, TestName = "MassiveEqualStats_Gives0.50%MissChance")] 
    [TestCase(-50, -50, 0.05f, TestName = "ExtremelySmallStats_Gives0.05%MissChance")] 
    public void CalculateMissChance_ReturnsExpectedValue(int attackerAtk, int defenderDex, float expectedMissChance)
    {
        _attacker.StatsComponent.ModifyStat(StatType.Atk, attackerAtk - 20);
        _defender.StatsComponent.ModifyStat(StatType.Dex, defenderDex - 10);
        
        float actualMissChance = DamageCalculator.CalculateMissChance(_attacker, _defender);
        
        AssertThat(actualMissChance).IsEqual(expectedMissChance);
    }
    
    [TestCase(ElementType.Null, ElementType.Null, 35)]
    public void CalculateDamage_DefenderStiffCheck(ElementType defenderElement, ElementType moveElement, int expectedDamage)
    {
        SetupDefenderElement(defenderElement);
        SetMoveStats(moveElement, 25, false);
        
        MakeDefenderStiff();
        
        (bool isDodged, bool isEnemyImmune, bool isCrit, int damageDealt) = 
            DamageCalculator.CalculateDamage(_attacker, _defender, _move);
            
        float actualMissChance = DamageCalculator.CalculateMissChance(_attacker, _defender);
        
        AssertThat(actualMissChance).IsEqual(0.0f);
        AssertThat(isDodged).IsFalse();
        AssertThat(isEnemyImmune).IsFalse();
        AssertThat(isCrit).IsFalse();
        AssertThat(damageDealt).IsEqual(expectedDamage);
    }
    
    // Helper Functions

    private void SetupDefenderElement(ElementType primaryElement, ElementType secondaryElement = ElementType.None)
    {
        _defenderStatResource.PrimaryElement = primaryElement;
        _defenderStatResource.SecondaryElement = secondaryElement;
        _defender.SetTestResource(_defenderStatResource);
    }

    private void MakeDefenderBroken()
    {
        _defender.StatsComponent.ModifyStat(StatType.Def, -20);
        _defender.DisabilityComponent.OnStatZeroed((int)StatType.Def);
    }

    private void MakeDefenderStiff()
    {
        _defender.StatsComponent.ModifyStat(StatType.Dex, -10);
        _defender.DisabilityComponent.OnStatZeroed((int)StatType.Dex);
    }
    private void SetMoveStats(ElementType moveType, int power, bool canCrit)
    {
        _move = new MoveResource()
        {
            MoveElement = moveType,
            Power = power,
            CanCrit = canCrit
        };
    }
    private void SetCharacterStatBase(Character character, StatType stat, int value)
    {
        character.CurrentStats[stat] = value;
    }
}