using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.Resources.MoveData
{
    [GlobalClass]
    public partial class MoveResource : Resource
    {
        [Export] public CompressedTexture2D Texture;
        [Export] public string Name;
        [Export] public int Power;
        [Export] public int Cost;
        [Export] public bool CanCrit = true;
    }
}

