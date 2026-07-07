using GdUnit4;
using static GdUnit4.Assertions;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;

namespace NeuralZeroProtocol.Tests.Scripts.Characters;

[TestSuite]
[RequireGodotRuntime]
public class TestHealthComponent
{
    private HealthComponent _healthComponent;
    private Character _character;
    private StatsComponent _statsComponent;

    [BeforeTest]
    public void Setup()
    {
        _character = AutoFree(new Character());

        _character.CurrentStats[StatType.Hp] = 500;

        _statsComponent = AutoFree(new StatsComponent());
        _healthComponent = AutoFree(new HealthComponent());
        
        _statsComponent.Initialize(_character);

        _character.StatsComponent = _statsComponent;
        _character.HealthComponent = _healthComponent;

    }
    
}