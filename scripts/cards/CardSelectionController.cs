using Godot;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Cards
{

	public partial class CardSelectionController : Node2D
	{	
		[Signal] public delegate void SelectionChangedEventHandler(Card oldCard, Card newCard);

		private CardSystem _cardSystem;
		private Card _selectedCard;

		public override void _Ready() => _cardSystem = GetNode<CardSystem>("..");
		
		public void OnCardClicked(Card card)
        {
            // Verify this card is the topmost under mouse
            var mousePos = GetGlobalMousePosition();

            var space = GetWorld2D().DirectSpaceState;

            var query = new PhysicsPointQueryParameters2D
            {
                Position = mousePos,
                CollideWithAreas = true,
                CollisionMask = 1
            };

            var results = space.IntersectPoint(query);

            Card highestCard = null;
            int highestZ = int.MinValue;

            foreach (var result in results)
            {
                var area = result["collider"].As<Area2D>();
                var newCard = area?.GetParent<Card>();

                if (newCard != null && newCard.ZIndex > highestZ)
                {
                    highestCard = newCard;
                    highestZ = newCard.ZIndex;
                }
            }
            if (highestCard != card) return;

            // Proceed with select/deselect
            if (card == _selectedCard) DeselectCard();

            else SelectCard(card);
        }


        private void SelectCard(Card card)
        {
            if (_selectedCard == card) return;

			Card oldCard = _selectedCard;

            // Deselect previous card with tween
            if (_selectedCard != null)
            {

                // Kill any hover or selection tween on old card
                _cardSystem.CardHoverController.KillAndRemoveTween(oldCard);
                
                // Tween scale back to 1.0
                Tween tween = CreateTween();
                tween.TweenProperty(oldCard, "scale", Vector2.One, 0.05f);

                _cardSystem.CardHoverController.AddTween(oldCard, tween);
                
                // Restore ZIndex
                oldCard.ZIndex = _cardSystem.OriginalZIndexes[oldCard];

                oldCard.UpdatePriority();
            }

            _selectedCard = card;

			EmitSignal(SignalName.SelectionChanged, oldCard, card);

            _cardSystem.CardHoverController.OnCardSelected(_selectedCard);

            // Tween scale to 1.1
            Tween selectTween = CreateTween();
            selectTween.TweenProperty(card, "scale", new Vector2(1.1f, 1.1f), 0.05f);

            _cardSystem.CardHoverController.AddTween(card, selectTween);

            // Set ZIndex and priority
            card.ZIndex = 10;
            card.UpdatePriority();
        }

        private void DeselectCard()
        {
            if (_selectedCard == null) return;

            Card oldSelected = _selectedCard;

            // Kill any tween on old selected
            _cardSystem.CardHoverController.KillAndRemoveTween(oldSelected);

            // Tween scale back to 1.0
            Tween tween = CreateTween();
            tween.TweenProperty(oldSelected, "scale", Vector2.One, 0.05f);

            _cardSystem.CardHoverController.AddTween(oldSelected, tween);

            // Restore ZIndex
            oldSelected.ZIndex = _cardSystem.OriginalZIndexes[oldSelected];
            oldSelected.UpdatePriority();

            _selectedCard = null;

			EmitSignal(SignalName.SelectionChanged, oldSelected, _selectedCard);
        }
	}
}

