using Godot;
using System;
using System.ComponentModel;

namespace NeuralZeroProtocol.Scripts.Cards
{
    public partial class CardSystem : Node2D
    {
        private Node2D _cardBeingDragged;
        private Vector2 _screenSize;

        public override void _Ready() => _screenSize = GetViewportRect().Size;

        public override void _Process(double delta) 
        {
            if (_cardBeingDragged != null)
            {
                var mousePos = GetGlobalMousePosition();
                var screenXClamped = Mathf.Clamp(mousePos.X, 0, _screenSize.X);
                var screenYClamped = Mathf.Clamp(mousePos.Y, 0, _screenSize.Y);

                _cardBeingDragged.Position = new Vector2(screenXClamped, screenYClamped);
            }
        }

        public override void _Input(InputEvent @event) 
        {
            if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (@event.IsPressed())
                {
                    GD.Print("Left Clicked");
                    var card = RaycastCheckCard();
                    if (card != null)
                    {
                        _cardBeingDragged = card;
                    }
                } 
                else
                {
                    _cardBeingDragged = null;
                }
            }
        }

        private Node2D RaycastCheckCard()
        {
            var spaceState = GetWorld2D().DirectSpaceState;
            var parameters = new PhysicsPointQueryParameters2D
            {
                Position = GetGlobalMousePosition(),
                CollideWithAreas = true,
                CollisionMask = 1
            };

            var result = spaceState.IntersectPoint(parameters);

            if(result.Count > 0)
            {
                var collider = (Node2D)result[0]["collider"];
                return collider.GetParent() as Node2D;
            }
            return null;
        }
    }
}

