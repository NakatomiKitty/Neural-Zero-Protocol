using Godot;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Resources.CharacterData
{   
    public enum StatTypes
    {
        Atk,
        Def,
        Dex,
        Int,
        Lck,
        Nrg,
    }
    
    [GlobalClass]
    public partial class CharacterStatsResources : Resource
    {

        [Export] public int AtkBase = 20;
        [Export] public int DefBase = 20;
        [Export] public int DexBase = 5;
        [Export] public int IntBase = 5;
        [Export] public int LckBase = 5;
        [Export] public int NrgBase = 5;

        public int GetBaseValues(StatTypes stat)
        {
            switch (stat)
            {
                case StatTypes.Atk:
                    return AtkBase;
                case StatTypes.Def:
                    return DefBase;
                case StatTypes.Dex:
                    return DexBase;
                case StatTypes.Int:
                    return IntBase;
                case StatTypes.Lck:
                    return LckBase;
                case StatTypes.Nrg:
                    return NrgBase;
                default:
                    return 0;
            }
        }
    }
}

