using GdUnit4;
using static GdUnit4.Assertions;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;

namespace NeuralZeroProtocol.Tests.Scripts.Characters;

[TestSuite]
[RequireGodotRuntime]
public class TestStatsComponent
{
    private Character _character;
    private StatsComponent _statsComponent;
    
    [BeforeTest]
    public void Setup()
    {
        _character = AutoFree(new Character());

        SetCharacterStatBase(StatType.Atk, 20);
        SetCharacterStatBase(StatType.Def, 20);
        SetCharacterStatBase(StatType.Dex, 10);
        SetCharacterStatBase(StatType.Int, 10);
        SetCharacterStatBase(StatType.Lck, 1);
        SetCharacterStatBase(StatType.Nrg, 5);
        
        _statsComponent = AutoFree(new StatsComponent());
        _statsComponent.Initialize(_character);
    }

    // Basic Modification Tests
    
    [TestCase]
    public void ModifyStat_StatIncreasesValueCorrectly()
    {
        _statsComponent.ModifyStat(StatType.Atk, 5);
        AssertThat(_character.CurrentStats[StatType.Atk]).IsEqual(25);
    }
    
    [TestCase]
    public void ModifyStat_StatDecreasesValueCorrectly()
    {
        _statsComponent.ModifyStat(StatType.Atk, -5);
        AssertThat(_character.CurrentStats[StatType.Atk]).IsEqual(15);
    }

    [TestCase]
    public void ModifyStat_StatClampsToZeroCorrectly()
    {
        _statsComponent.ModifyStat(StatType.Nrg, -10);
        AssertThat(_character.CurrentStats[StatType.Nrg]).IsEqual(0);
    }
    
    // Event Tests
    [TestCase]
    public void ModifyStat_FiresStatZeroed_WhenStatReachesZero()
    {
        bool zeroedFired = false;
        int firedStat = -1;

        _statsComponent.StatZeroed += (stat) =>
        {
            zeroedFired = true;
            firedStat = stat;
        };
        
        _statsComponent.ModifyStat(StatType.Atk, -20);
        
        AssertThat(zeroedFired).IsTrue();
        AssertThat(firedStat).IsEqual((int)StatType.Atk);
    }
    
    [TestCase]
    public void ModifyStat_DoesNotFireStatZeroed_WhenStatStaysAboveZero()
    {
        bool zeroedFired = false;

        _statsComponent.StatZeroed += (stat) => zeroedFired = true;
        
        _statsComponent.ModifyStat(StatType.Def, -2);
        
        AssertThat(zeroedFired).IsFalse();
    }
    
    [TestCase]
    public void ModifyStat_FiresStatRecovered_WhenStatRisesAboveZero()
    {
        // Force Def to Zero first
        _statsComponent.ModifyStat(StatType.Def, -20);
        
        bool recoveredFired = false;
        int firedStat = -1;

        _statsComponent.StatRecovered += (stat) =>
        {
            recoveredFired = true;
            firedStat = stat;
        };
        
        _statsComponent.ModifyStat(StatType.Def, 1);
        
        AssertThat(recoveredFired).IsTrue();
        AssertThat(firedStat).IsEqual((int)StatType.Def);
        AssertThat(_character.CurrentStats[StatType.Def]).IsEqual(1);
    }
    
    [TestCase]
    public void ModifyStat_DoesNotFireStatRecovered_WhenStatWasAlreadyAboveZero()
    {
        bool recoveredFired = false;

        _statsComponent.StatRecovered += (stat) => recoveredFired = true;
        
        _statsComponent.ModifyStat(StatType.Atk, 5);

        AssertThat(recoveredFired).IsFalse();
    }
    
    // GetStat Edge Cases
    [TestCase]
    public void GetStat_ReturnsDefaultValue_WhenStatNotPresent()
    {
        Character emptyChar = AutoFree(new Character());
        StatsComponent emptyStats = AutoFree(new StatsComponent());
        emptyStats.Initialize(emptyChar);

        AssertThat(emptyStats.GetStat(StatType.Hp)).IsEqual(0);
    }
    
    // Helper Functions
    private void SetCharacterStatBase(StatType stat, int value)
    {
        _character.CurrentStats[stat] = value;
    }
}