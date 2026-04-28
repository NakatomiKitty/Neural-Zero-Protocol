using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    public partial class PlayerCharacter : Character
    {
        public ActionValidatorComponent ActionValidatorComponent;

        public override void _Ready() 
        {
            ActionValidatorComponent = GetNode<ActionValidatorComponent>("ActionValidatorComponent");
            base._Ready();
            
            ActionValidatorComponent.Initialize(this);
        }

    }
}

