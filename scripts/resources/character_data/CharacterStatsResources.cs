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

        [Export] public int atkBase = 20;
        [Export] public int defBase = 20;
        [Export] public int dexBase = 5;
        [Export] public int intBase = 5;
        [Export] public int lckBase = 5;
        [Export] public int nrgBase = 5;

        public int GetBaseValues(StatTypes stat)
        {
            switch (stat)
            {
                case StatTypes.Atk:
                    return atkBase;
                case StatTypes.Def:
                    return defBase;
                case StatTypes.Dex:
                    return dexBase;
                case StatTypes.Int:
                    return intBase;
                case StatTypes.Lck:
                    return lckBase;
                case StatTypes.Nrg:
                    return nrgBase;
                default:
                    return 0;
            }
        }
    }
}

