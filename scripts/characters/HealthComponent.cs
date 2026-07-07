using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.Characters;

[GlobalClass]
public partial class HealthComponent : Node
{
    // TODO: USED FOR HEALTHBAR ↓!
    public event Action<int, int> HealthChanged; // int currentHealth, int previousHealth
    public event Action<Character> Died; // Character parent

    private Character _character;
    public int MaxHealth;
    public int CurrentHealth;

    public void Initialize(Character character) => _character = character;
    public void InitializeHealth(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        HealthChanged?.Invoke(CurrentHealth, CurrentHealth);
    }

    public void TakeDamage(int damage)
    {
        int oldHealth = CurrentHealth;

		CurrentHealth = Math.Max(0, CurrentHealth - damage);

        GD.Print($"{_character.Name} got hit! ({oldHealth} → {CurrentHealth}) [Received: {damage} damage!]");

        HealthChanged?.Invoke(CurrentHealth, oldHealth);

        if (CurrentHealth <= 0)
        {
            GD.Print("Health reached zero! You died!");
            
            Died?.Invoke(_character);
        }
    }

    public void HealHealth(int heal)
    {
        if (CurrentHealth <= 0) return;

        int oldHealth = CurrentHealth;

        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + heal);

        GD.Print($"Health got healed! ({oldHealth} → {CurrentHealth}) [Healed: {heal } hp!]");

        HealthChanged?.Invoke(CurrentHealth, oldHealth);
    }
}

