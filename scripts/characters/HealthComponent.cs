using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.Characters;

[GlobalClass]
public partial class HealthComponent : Node
{
    // TODO: USED FOR HEALTHBAR ↓!
    public event Action<int, int> HealthChanged; // int currentHealth, int maxHealth
    public event Action<Character> Died; // Character parent

    private int _maxHealth;
    private int _currentHealth;


    public void InitializeHealth(int maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void TakeDamage(int damage)
    {
        int oldHealth = _currentHealth;

		_currentHealth = Math.Max(0, _currentHealth - damage);

        GD.Print($"{GetParent().Name} got hit! ({oldHealth} → {_currentHealth}) [Received: {damage} damage!]");

        HealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth <= 0)
        {
            GD.Print("Health reached zero! You died!");
            
            Died?.Invoke((Character)GetParent());
        }
    }

    public void HealHealth(int heal)
    {
        if (_currentHealth <= 0) return;

        int oldHealth = _currentHealth;

        _currentHealth = Math.Min(_maxHealth, _currentHealth + heal);

        GD.Print($"Health got healed! ({oldHealth} → {_currentHealth}) [Healed: {heal } hp!]");

        HealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
}

