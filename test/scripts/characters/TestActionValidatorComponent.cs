using System;
using GdUnit4;
using Godot.Collections;
using static GdUnit4.Assertions;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Tests.Scripts.Characters;

[TestSuite]
[RequireGodotRuntime]
public class TestActionValidatorComponent
{
    private Character _character;
    private StatsComponent _statsComponent;
    private MovesetComponent _movesetComponent;
    private DisabilityComponent _disabilityComponent;
    private ActionValidatorComponent _actionValidatorComponent;

    [BeforeTest]
    public void Setup()
    {
        _character = AutoFree(new Character());
        
        SetCharacterStatBase(StatType.Hp, 500);
        SetCharacterStatBase(StatType.Atk, 20);
        SetCharacterStatBase(StatType.Def, 20);
        SetCharacterStatBase(StatType.Dex, 10);
        SetCharacterStatBase(StatType.Int, 10);
        SetCharacterStatBase(StatType.Lck, 1);
        SetCharacterStatBase(StatType.Nrg, 5);
        
        _statsComponent = AutoFree(new StatsComponent());
        _disabilityComponent = AutoFree(new DisabilityComponent());
        _actionValidatorComponent = AutoFree(new ActionValidatorComponent());
        _movesetComponent = AutoFree(new MovesetComponent());
        
        _statsComponent.Initialize(_character);
        _actionValidatorComponent.Initialize(_character);
        _movesetComponent.Initialize(_character);

        _character.MovesetComponent = _movesetComponent;
        _character.StatsComponent = _statsComponent;
        _character.DisabilityComponent = _disabilityComponent;
        _character.ActionValidatorComponent = _actionValidatorComponent;
    }
    
    // Basic Flag Tests

    [TestCase]
    public void CanSkip_ConfirmIsAlwaysTrue()
    {
        bool canSkipFlag = _character.ActionValidatorComponent.CanSkip();

        AssertThat(canSkipFlag).IsTrue();
    }
    
    [TestCase]
    public void CanDefend_ConfirmIsAlwaysTrue()
    {
        bool canDefendFlag = _character.ActionValidatorComponent.CanDefend();

        AssertThat(canDefendFlag).IsTrue();
    }

    [TestCase]
    public void IsNotExhausted_NoDisability_ReturnsTrue()
    {
        bool result = _character.ActionValidatorComponent.IsNotExhausted();
        AssertThat(result).IsTrue();
    }

    [TestCase]
    public void IsNotExhausted_ExhaustedDisability_ReturnsFalse()
    {
        MakeCharacterExhausted();
        bool result = _character.ActionValidatorComponent.IsNotExhausted();
        AssertThat(result).IsFalse();
    }

    [TestCase(1, true, TestName = "CanUseMove_MoveCostIsLessThanCharacterNrg_True")]
    [TestCase(5, true, TestName = "CanUseMove_MoveCostIsEqualToCharacterNrg_True")]
    [TestCase(6, false, TestName = "CanUseMove_MoveCostIsMoreThanCharacterNrg_False")]
    public void CanUseMove_CheckHasEnoughNrg(int cost, bool expectedOutcome)
    {
        MoveResource move = AutoFree(new MoveResource() { Cost = cost });

        bool CanUseMoveFlag = _character.ActionValidatorComponent.CanUseMove(move);

        AssertThat(CanUseMoveFlag).IsEqual(expectedOutcome);
    }
    
    // Helper Functions
    private void SetCharacterStatBase(StatType stat, int value)
    {
        _character.CurrentStats[stat] = value;
    }
    
    private void MakeCharacterExhausted()
    {
        _character.StatsComponent.ModifyStat(StatType.Nrg, -5);
        _character.DisabilityComponent.OnStatZeroed((int)StatType.Nrg);
    }
}