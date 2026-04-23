using Godot;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Resources.CharacterData
{   
    public enum StatTypes
    {
        ATK,
        DEF,
        DEX,
        INT,
        LCK,
        NRG,
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
                case StatTypes.ATK:
                    return atkBase;
                case StatTypes.DEF:
                    return defBase;
                case StatTypes.DEX:
                    return dexBase;
                case StatTypes.INT:
                    return intBase;
                case StatTypes.LCK:
                    return lckBase;
                case StatTypes.NRG:
                    return nrgBase;
                default:
                    return 0;
            }
        }
    }
}

