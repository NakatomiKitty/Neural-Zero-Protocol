using System;
using GdUnit4;
using Godot.Collections;
using static GdUnit4.Assertions;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;

namespace NeuralZeroProtocol.Tests.Scripts.Characters;

[TestSuite]
[RequireGodotRuntime]
public class TestDisabilityComponent
{
    private DisabilityComponent _disabilityComponent;

    private static readonly Dictionary<StatType, DisabilityTypes> StatToDisability = new()
    {
        { StatType.Atk, DisabilityTypes.Fragile },
        { StatType.Def, DisabilityTypes.Broken },
        { StatType.Dex, DisabilityTypes.Stiff },
        { StatType.Int, DisabilityTypes.Mindless },
        { StatType.Lck, DisabilityTypes.Cursed },
        { StatType.Nrg, DisabilityTypes.Exhausted },
    };
    
    [BeforeTest]
    public void Setup()
    {
        _disabilityComponent = AutoFree(new DisabilityComponent());
    }
    
    // Disability State Tests

    [TestCase]
    public void VerifyZeroDisabilitiesOnStartup()
    {
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            if (stat is StatType.Hp) return;
            AssertThat(HasDisability(stat)).IsFalse();
        }
    }
    
    [TestCase(StatType.Atk, TestName = "OnStatZeroed_FragileIsActivated")]
    [TestCase(StatType.Def, TestName = "OnStatZeroed_BrokenIsActivated")]
    [TestCase(StatType.Dex, TestName = "OnStatZeroed_StiffIsActivated")]
    [TestCase(StatType.Int, TestName = "OnStatZeroed_MindlessIsActivated")]
    [TestCase(StatType.Lck, TestName = "OnStatZeroed_CursedIsActivated")]
    [TestCase(StatType.Nrg, TestName = "OnStatZeroed_ExhaustedIsActivated")]
    public void OnStatZeroed_VerifyDisabilityIsActivated(StatType statType)
    {
        _disabilityComponent.OnStatZeroed((int)statType);
        
        AssertThat(HasDisability(statType)).IsTrue();
    }
    
    [TestCase(StatType.Atk)]
    public void OnStatZeroed_DisabilityIdempotencyCheck(StatType statType)
    {
        _disabilityComponent.OnStatZeroed((int)statType);
        _disabilityComponent.OnStatZeroed((int)statType);
        
        AssertThat(HasDisability(statType)).IsTrue();
    }
    
    [TestCase(StatType.Atk, TestName = "OnStatRecovered_FragileIsDeactivated")]
    [TestCase(StatType.Def, TestName = "OnStatRecovered_BrokenIsDeactivated")]
    [TestCase(StatType.Dex, TestName = "OnStatRecovered_StiffIsDeactivated")]
    [TestCase(StatType.Int, TestName = "OnStatRecovered_MindlessIsDeactivated")]
    [TestCase(StatType.Lck, TestName = "OnStatRecovered_CursedIsDeactivated")]
    [TestCase(StatType.Nrg, TestName = "OnStatRecovered_ExhaustedIsDeactivated")]
    public void OnStatRecovered_VerifyDisabilityIsDeactivated(StatType statType)
    {
        _disabilityComponent.OnStatZeroed((int)statType);
        _disabilityComponent.OnStatRecovered((int)statType);
        
        AssertThat(HasDisability(statType)).IsFalse();
    }
    
    [TestCase(StatType.Atk)]
    public void OnStatRecovered_RecoveryWithoutActivation(StatType statType)
    {
        _disabilityComponent.OnStatRecovered((int)statType);
        
        AssertThat(HasDisability(statType)).IsFalse();
    }

    [TestCase]
    public void OnStatZeroedAndRecovered_VerifyDisabilitiesAreTrackedIndependently()
    {
        _disabilityComponent.OnStatZeroed((int)StatType.Atk);
        _disabilityComponent.OnStatZeroed((int)StatType.Def);
        
        _disabilityComponent.OnStatRecovered((int)StatType.Def);
        
        AssertThat(HasDisability(StatType.Atk)).IsTrue();
        AssertThat(HasDisability(StatType.Def)).IsFalse();
    }

    // Helper Functions
    private bool HasDisability(StatType statType) => _disabilityComponent.HasDisability(StatToDisability[statType]);
}