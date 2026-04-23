using Godot;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;
using System;
using System.Collections.Generic;

// StatsComponent handles a character's 6 stats (ATK, DEF, DEX, INT, LCK, NRG).
// It stores current values in a dictionary, allows modification with delta,
// and emits signals when stats hit 0 (DeadZone) or recover above 0.

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class StatsComponent : Node
    {
        // Declaration of Signals
        [Signal] public delegate void StatChangedEventHandler(int stat, int newValue);
        [Signal] public delegate void StatZeroedEventHandler(int stat);
        [Signal] public delegate void StatRecoveredEventHandler(int stat);

        [Export] private CharacterStatsResources _characterStatsResources;

        private Dictionary<StatTypes, int> _currentStats = new Dictionary<StatTypes, int>();

        public override void _Ready() {
            InitializeFromResource(_characterStatsResources);
        }

        private void InitializeFromResource(CharacterStatsResources resource)
        {
            _currentStats.Clear(); // clear just in case of re-initialization
            foreach (StatTypes stat in Enum.GetValues<StatTypes>()) // loop through every StatType Values
            {
                // Get the basevalues from the resource then add it to the dictionary
                int baseValue = resource.GetBaseValues(stat); 
                _currentStats[stat] = baseValue;
            }
        }

        public int GetStat(StatTypes stat) // this retrieves the old value
        {
            if (_currentStats.TryGetValue(stat, out int value))
            {
                return value;
            }
            return 0;
        }

        public void ModifyStat(StatTypes stat, int delta)
        {
            // If you forget to assign a CharacterStatsResources in the editor, the game would crash when a character uses this component.
            // Adding this here will atleast notifies us early :)
            if (_characterStatsResources == null)
            {
                GD.PushWarning("NO CHARACTERSTATSRESOURCE SELECTED");
            }

            var oldValue = GetStat(stat);
            var currentValue = Math.Max(0, oldValue + delta); // Clamp to prevent going past below zero

            _currentStats[stat] = currentValue;

            // Tell any listening nodes that this stat's value has changed.
            // Example: If DEX goes from 5 to 1, we send: (2(stat), 1(newValue)) because DEX = 2 in the enum.
            EmitSignal(SignalName.StatChanged, (int)stat, currentValue);

            // If the stat was positive and now becomes exactly zero, it means the character just entered the "DeadZone".
            // Example: ATK drops from 5 to 0 → character becomes "Fragile". etc. etc. etc.
            if (oldValue > 0 && currentValue == 0) 
            {
                EmitSignal(SignalName.StatZeroed, (int)stat);
            }

            // If the stat was zero and was recently recovered, it means the character has gotten out of the "DeadZone"
            // Character is no more fragile YIPPIEEE
            else if (oldValue == 0 && currentValue > 0) 
            {
                EmitSignal(SignalName.StatRecovered, (int)stat);
            }
        }
    }
}

