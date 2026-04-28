using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.UI
{
    public enum ActionType
    {
        Move,
        Defend,
        Skip
    }
    [GlobalClass]
    public partial class ActionPanel : Node
    {
        [Signal] public delegate void ActionSelectedEventHandler(int actionType);

        private Panel _buttonPanel;

        public override void _Ready() 
        {
            _buttonPanel = GetNode<Panel>("Panel");

            foreach(Node child in _buttonPanel.GetChildren())
            {
                if (child is Button button)
                {
                    button.Pressed += () => OnAnyButtonPressed(button);
                }
            }
        }

        private void OnAnyButtonPressed(Button button)
        {
            ActionType action = button.Name.ToString() switch
            {
                "MoveButton" => ActionType.Move,
                "DefendButton" => ActionType.Defend,
                "SkipButton" => ActionType.Skip,
                _ => ActionType.Skip
            };

            GD.Print(action);
            EmitSignal(SignalName.ActionSelected, (int)action);
        }
    }
}

