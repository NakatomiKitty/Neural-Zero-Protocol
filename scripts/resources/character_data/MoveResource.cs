using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.Resources.CharacterData
{
    [GlobalClass]
    public partial class MoveResource : Resource
    {
        [Export] public string Name;
        [Export] public int Power;
        [Export] public int Cost;
        [Export] public bool CanCrit = true;
    }
}

