using GdUnit4;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Characters;
using static GdUnit4.Assertions;
using NeuralZeroProtocol.Scripts.Combat;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Tests.Scripts.Combat;

[TestSuite]
[RequireGodotRuntime]
public class TestBattleManager
{
    private BattleManager _battleManager;
    private PlayerCharacter _playerCharacter;
    private MovesetComponent _movesetComponent;

    [BeforeTest]
    public void Setup()
    {
        _battleManager = AutoFree(new BattleManager());
        _playerCharacter = AutoFree(new PlayerCharacter());
        _movesetComponent = AutoFree(new MovesetComponent());
        _playerCharacter.MovesetComponent = _movesetComponent;
    }

    [TestCase]
    public void GetSelectedMoves_ReturnsUpTo5Moves()
    {
        _movesetComponent.Moves = CreateMoves(7);

        Array<MoveResource> result = _battleManager.GetSelectedMovesFromCurrentChar(_playerCharacter);

        AssertThat(result.Count).IsEqual(5);
    }
    
    [TestCase]
    public void GetSelectedMoves_ReturnsAllMoves_WhenLessThan5()
    {
        _movesetComponent.Moves = CreateMoves(3);

        Array<MoveResource> result = _battleManager.GetSelectedMovesFromCurrentChar(_playerCharacter);

        AssertThat(result.Count).IsEqual(3);
    }
    
    [TestCase]
    public void GetSelectedMoves_ReturnsEmpty()
    {
        _movesetComponent.Moves = CreateMoves(0);

        Array<MoveResource> result = _battleManager.GetSelectedMovesFromCurrentChar(_playerCharacter);

        AssertThat(result.Count).IsEqual(0);
        AssertThat(result).IsNotNull();
    }
    
    [TestCase]
    public void GetSelectedMoves_ReturnsCopy_NotOriginal()
    {
        _movesetComponent.Moves = CreateMoves(5);
        Array<MoveResource> original = _movesetComponent.Moves;
        
        Array<MoveResource> result = _battleManager.GetSelectedMovesFromCurrentChar(_playerCharacter);
        
        AssertThat(result).IsNotSame(original);
    }
    
    // Helper Functions
    
    private Array<MoveResource> CreateMoves(int count)
    {
        var moves = new Array<MoveResource>();
        for (int i = 0; i < count; i++)
        {
            moves.Add(new MoveResource { Name = $"Move_{i}" });
        }
        return moves;
    }
}