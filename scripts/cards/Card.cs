using Godot;
using GodotUtilities;
using System;

namespace NeuralZeroProtocol.Scripts.Cards
{
    [GlobalClass]
    [Scene]
    public partial class Card : Node2D
    {
        [Signal] public delegate void HoveredEventHandler(Card card);
        [Signal] public delegate void NotHoveredEventHandler(Card card);
        [Signal] public delegate void ClickedEventHandler(Card card);
        [Node] private Area2D _area2D;

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated) WireNodes();
        }

        public override void _Ready() 
        {
            _area2D.Priority = ZIndex;
            _area2D.MouseEntered += OnMouseEntered;
            _area2D.MouseExited += OnMouseExited;
            _area2D.InputEvent += OnMouseClicked;
        }

        private void OnMouseClicked(Node viewport, InputEvent @event, long shapeIdx)
        {
            if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.IsPressed())
            {
                EmitSignal(SignalName.Clicked, this);
            }
        }

        private void OnMouseEntered()
        {
            EmitSignal(SignalName.Hovered, this);
        }

        private void OnMouseExited()
        {
            EmitSignal(SignalName.NotHovered, this);
        }
    }
}

