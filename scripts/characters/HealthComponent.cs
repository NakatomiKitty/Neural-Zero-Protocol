using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class HealthComponent : Node
    {
        [Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
		[Signal] public delegate void DiedEventHandler();

        private StatsComponent _statsComponent;
        private int _currentHealth;
        public override void _Ready() 
        {
            _statsComponent = GetNode<StatsComponent>("../StatsComponent");

            // Check if null
			if (_statsComponent == null)
			{
				GD.PushWarning("StatsComponent not loaded lmao");
			}

            _currentHealth = _statsComponent.GetStat(StatTypes.Hp);
        }

        public void TakeDamage(int damage)
        {
            var maxHealth = _statsComponent.GetStat(StatTypes.Hp);
            int oldHealth = _currentHealth;

			_currentHealth = Math.Max(0, _currentHealth - damage);

            GD.Print($"You got hit! ({oldHealth} → {_currentHealth}) [Received: {damage} damage!]");

            EmitSignal(SignalName.HealthChanged, _currentHealth, maxHealth);

            if (_currentHealth <= 0)
            {
                GD.Print("Health reached zero! You died!");

				EmitSignal(SignalName.Died);
            }
        }

        public void HealHealth(int heal)
        {
            if (_currentHealth <= 0) return;

            var maxHealth = _statsComponent.GetStat(StatTypes.Hp);
            int oldHealth = _currentHealth;

            _currentHealth = Math.Min(maxHealth, _currentHealth + heal);

            GD.Print($"Health got healed! ({oldHealth} → {_currentHealth}) [Healed: {heal } hp!]");

            EmitSignal(SignalName.HealthChanged, _currentHealth, maxHealth);
        }
    }
}

