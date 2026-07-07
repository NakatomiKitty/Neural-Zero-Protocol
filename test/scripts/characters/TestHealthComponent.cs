using GdUnit4;
using NeuralZeroProtocol.Autoloads;
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

    [BeforeTest]
    public void Setup()
    {
        _character = AutoFree(new Character());
        _healthComponent = AutoFree(new HealthComponent());

        _character.HealthComponent = _healthComponent;
        _healthComponent.Initialize(_character);
        _healthComponent.InitializeHealth(500);
    }

    // Basic modification tests
    
    [TestCase]
    public void TakeDamage_ReducesHealthCorrectly()
    {
        _healthComponent.TakeDamage(100);

        AssertThat(_healthComponent.MaxHealth).IsEqual(500);
        AssertThat(_healthComponent.CurrentHealth).IsEqual(400);
    }

    [TestCase]
    public void TakeDamage_ClampsToZero()
    {
        _healthComponent.TakeDamage(600);
        
        AssertThat(_healthComponent.MaxHealth).IsEqual(500);
        AssertThat(_healthComponent.CurrentHealth).IsEqual(0);
    }

    [TestCase]
    public void HealHealth_IncreasesHealthCorrectly()
    {
        _healthComponent.TakeDamage(100);
        _healthComponent.HealHealth(50);

        AssertThat(_healthComponent.MaxHealth).IsEqual(500);
        AssertThat(_healthComponent.CurrentHealth).IsEqual(450);
    }

    [TestCase]
    public void HealHealth_ClampsToMaxHealth()
    {
        _healthComponent.TakeDamage(200);
        _healthComponent.HealHealth(500);
        
        AssertThat(_healthComponent.MaxHealth).IsEqual(500);
        AssertThat(_healthComponent.CurrentHealth).IsEqual(500);
    }
    
    [TestCase]
    public void HealHealth_DoesNothingWhenDead()
    {
        _healthComponent.TakeDamage(600);
        _healthComponent.HealHealth(500);
        
        AssertThat(_healthComponent.MaxHealth).IsEqual(500);
        AssertThat(_healthComponent.CurrentHealth).IsEqual(0);
    }
    
    // Event tests

    [TestCase]
    public void FireDiedEvent_WhenHealthReachesZero()
    {
        bool diedFired = false;

        _healthComponent.Died += _ =>
        {
            diedFired = true;
        };
        
        _healthComponent.TakeDamage(600);
        
        AssertThat(diedFired).IsTrue();
    }
    
    [TestCase]
    public void FireHealthChanged_WhenTakingDamage()
    {
        int firedCurrentHealth = 0;
        int firedOldHealth = 0;
        bool healthChangedFired = false;
        
        _healthComponent.HealthChanged += (currentHealth, oldHealth) =>
        {
            firedCurrentHealth = currentHealth;
            firedOldHealth = oldHealth;
            healthChangedFired = true;
        };
        
        _healthComponent.TakeDamage(100);

        AssertThat(firedCurrentHealth).IsEqual(400);
        AssertThat(firedOldHealth).IsEqual(500);
        AssertThat(healthChangedFired).IsTrue();
    }
    
    [TestCase]
    public void FireHealthChanged_WhenHealing()
    {
        int firedCurrentHealth = 0;
        int firedOldHealth = 0;
        bool healthChangedFired = false;
        
        _healthComponent.HealthChanged += (currentHealth, oldHealth) =>
        {
            firedCurrentHealth = currentHealth;
            firedOldHealth = oldHealth;
            healthChangedFired = true;
        };
        _healthComponent.TakeDamage(100);
        _healthComponent.HealHealth(10);

        AssertThat(firedCurrentHealth).IsEqual(410);
        AssertThat(firedOldHealth).IsEqual(400);
        AssertThat(healthChangedFired).IsTrue();
    }
    
    [TestCase]
    public void FireHealthChanged_OnInitialization()
    {
        int firedCurrentHealth = 0;
        int firedOldHealth = 0;
        bool healthChangedFired = false;
        
        _healthComponent.HealthChanged += (currentHealth, oldHealth) =>
        {
            firedCurrentHealth = currentHealth;
            firedOldHealth = oldHealth;
            healthChangedFired = true;
        };

        _healthComponent.InitializeHealth(500);
        
        AssertThat(firedCurrentHealth).IsEqual(500);
        AssertThat(firedOldHealth).IsEqual(500);
        AssertThat(healthChangedFired).IsTrue();
    }
}