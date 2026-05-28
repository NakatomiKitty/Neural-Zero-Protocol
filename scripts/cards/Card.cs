using Godot;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using System;

namespace NeuralZeroProtocol.Scripts.Cards
{
    /// <summary>
    /// Represents a single card in the hand. Handles mouse input and emits signals
    /// for hovering, unhovering, and click.
    /// </summary>
    [GlobalClass]
    [Scene]
    public partial class Card : Node2D
    {
        [Signal] public delegate void HoveredEventHandler(Card card);
        [Signal] public delegate void NotHoveredEventHandler(Card card);
        [Signal] public delegate void ClickedEventHandler(Card card);
        [Node] private Area2D _area2D;
        [Node] private Sprite2D _cardImage;

        public MoveResource MoveData;

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated) WireNodes();
        }

        public override void _Ready() 
        {
            _area2D.MouseEntered += OnMouseEntered;
            _area2D.MouseExited += OnMouseExited;
            _area2D.InputEvent += OnMouseClicked;
            
            UpdatePriority();
        }
        
        private void OnMouseEntered() => EmitSignal(SignalName.Hovered, this);
        private void OnMouseExited() => EmitSignal(SignalName.NotHovered, this);
        
        // Higher ZIndex means higher priority which means it receives clicks first.

        public void UpdatePriority() => _area2D.Priority = ZIndex;
        
        public void ChangeCardSkin(MoveResource move)
        {
            MoveData = move;
            _cardImage.Frame = (int)MoveData.MoveElement * 2 + (int)MoveData.ActionType;
        }
        
        private void OnMouseClicked(Node viewport, InputEvent @event, long shapeIdx)
        {
            if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.IsPressed())
            {
                // Passes itself to the signal
                EmitSignal(SignalName.Clicked, this);
            }
        }
        
        
    }
}