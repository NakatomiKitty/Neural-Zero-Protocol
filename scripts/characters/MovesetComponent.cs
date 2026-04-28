using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using NeuralZeroProtocol.Scripts.Resources.CharacterData;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class MovesetComponent : Node
    {
        [Export] public Array<MoveResource> Moves = new Array<MoveResource>();
        
        private Character _character;

        public void Initialize(Character character) => _character = character;

        public override void _Ready() 
        {
			if (_character == null)
			{
				GD.PushError($"Character is not loaded in!");
			}
		}
        
        public Array<MoveResource> GetMoves() => Moves;

        public bool CheckNrg(MoveResource move) => _character.StatsComponent.GetStat(StatTypes.Nrg) >= move.Cost;
        
        public void SpendNrg(MoveResource move)
        {
            if (CheckNrg(move)) _character.StatsComponent.ModifyStat(StatTypes.Nrg, -move.Cost);
        }


    }
}

