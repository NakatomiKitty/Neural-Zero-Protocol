using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class HealthComponent : Node
    {
        [Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
		[Signal] public delegate void DiedEventHandler(Character parent);

        private int _maxHealth;
        private int _currentHealth;


        public void InitializeHealth(int maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);
        }

        public void TakeDamage(int damage)
        {
            int oldHealth = _currentHealth;

			_currentHealth = Math.Max(0, _currentHealth - damage);

            GD.Print($"{GetParent().Name} got hit! ({oldHealth} → {_currentHealth}) [Received: {damage} damage!]");

            EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                GD.Print("Health reached zero! You died!");

				EmitSignal(SignalName.Died, GetParent());
            }
        }

        public void HealHealth(int heal)
        {
            if (_currentHealth <= 0) return;

            int oldHealth = _currentHealth;

            _currentHealth = Math.Min(_maxHealth, _currentHealth + heal);

            GD.Print($"Health got healed! ({oldHealth} → {_currentHealth}) [Healed: {heal } hp!]");

            EmitSignal(SignalName.HealthChanged, _currentHealth, _maxHealth);
        }
    }
}

