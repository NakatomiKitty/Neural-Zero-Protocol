using Godot;
using System;

namespace NeuralZeroProtocol.Scripts.UI
{
    public enum ActionType
    {
        Attack,
        Defend,
        Skip
    }
    [GlobalClass]
    public partial class ActionPanel : Node
    {
        [Signal] public delegate void ActionSelectedEventHandler();

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
                "PlaceholderAttack" => ActionType.Attack,
                "PlaceholderDefend" => ActionType.Defend,
                "PlaceholderSkip" => ActionType.Skip,
                _ => ActionType.Skip
            };
            EmitSignal(SignalName.ActionSelected, (int)action);
        }
    }
}

