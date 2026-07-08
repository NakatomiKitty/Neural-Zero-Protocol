using System.Linq;
using GdUnit4;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using static GdUnit4.Assertions;
namespace NeuralZeroProtocol.Tests.Scripts.Resources.CharacterData;

[TestSuite]
[RequireGodotRuntime]
public class TestMovesetComponent
{
    private MovesetComponent _movesetComponent;
    private Character _character;
    private StatsComponent _statsComponent;

    [BeforeTest]
    public void Setup()
    {
        _character = AutoFree(new Character());

        _character.CurrentStats[StatType.Nrg] = 5;
        
        _statsComponent = AutoFree(new StatsComponent());
        _movesetComponent = AutoFree(new MovesetComponent());
        
        _statsComponent.Initialize(_character);
        _movesetComponent.Initialize(_character);
        _character.StatsComponent = _statsComponent;
        _character.MovesetComponent = _movesetComponent;
    }

    // Basic Array Test
    [TestCase(5)]
    public void GetMove_ReturnAllMovesInArray(int amountOfMoves)
    {
        for (int i = 0; i < amountOfMoves; i++)
        {
            MoveResource move = CreateMove(1);
            
            _movesetComponent.Moves.Add(move);
        }

        int count = _movesetComponent.GetMoves().Count;

        AssertThat(amountOfMoves).IsEqual(count);
    }
    
    // Basic Flag Tests
    [TestCase(1, true, TestName = "ReturnsTrue_WhenCostIsLessThanNrgStat")]
    [TestCase(5, true, TestName = "ReturnsTrue_WhenCostIsEqualToNrgStat")]
    [TestCase(10, false, TestName = "ReturnsFalse_WhenCostIsGreaterThanNrgStat")]
    public void HasEnoughNrg_ReturnsCorrectFlagState(int nrgCost, bool expectedOutcome)
    {
        MoveResource move = CreateMove(nrgCost);
        bool result = _movesetComponent.HasEnoughNrg(move);

        AssertThat(result).IsEqual(expectedOutcome);
    }
    
    // Basic Modification Tests
    [TestCase]
    public void SpendNrg_ReducesNrgCorrectly()
    {
        MoveResource move = CreateMove(1);
        _movesetComponent.SpendNrg(move);

        int currentNrg = _character.CurrentStats[StatType.Nrg];

        AssertThat(currentNrg).IsEqual(4);
    }

    [TestCase]
    public void SpendNrg_DoesNothingWhenNotEnoughNrg()
    {
        _character.StatsComponent.ModifyStat(StatType.Nrg, -5);
        
        MoveResource move = CreateMove(1);
        _movesetComponent.SpendNrg(move);

        int currentNrg = _character.CurrentStats[StatType.Nrg];

        AssertThat(currentNrg).IsEqual(0);
    }
    
    // Helper Functions
    private MoveResource CreateMove(int nrgCost)
    {
        MoveResource move = AutoFree(new MoveResource() { Cost = nrgCost});

        return move;
    }
}