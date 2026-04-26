using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class Character : Node2D
    {
        public StatsComponent StatsComponent;
        public DisabilityComponent DisabilityComponent;
        public HealthComponent HealthComponent;

    }
}
