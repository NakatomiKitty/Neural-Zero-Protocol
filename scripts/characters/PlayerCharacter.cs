using Godot;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class PlayerCharacter : Character
    {
        public ActionValidatorComponent ActionValidatorComponent;

        public override void _EnterTree() 
        {
            ActionValidatorComponent = GetNode<ActionValidatorComponent>("ActionValidatorComponent");
            base._EnterTree();
            
            ActionValidatorComponent.Initialize(this);
        }
    }
}

