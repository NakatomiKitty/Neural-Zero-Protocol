using Godot;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Cards
{
	/// <summary>
    /// Handles card selection/deselection logic, including visual feedback (scale, ZIndex, priority).
    /// When selection changes, emits a signal to notify other controllers.
    /// </summary>
	public partial class CardSelectionController : Node2D
	{	
		[Signal] public delegate void SelectionChangedEventHandler(Card oldCard, Card newCard);

		private CardSystem _cardSystem;
		private Card _selectedCard;

		public override void _Ready() => _cardSystem = GetNode<CardSystem>("..");
		
		/// <summary>
		/// This is a hefty one, Called when any card is clicked
		/// Then performs a task (a raycast) to find the topmost card (highestCard) and only
		/// proceeds if the clicked card is indeed the highest (prevents jank shit when cards overlap)
		/// Works alongside Card.UpdatePriority.
		/// </summary>
		public void OnCardClicked(Card card)
        {
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

				// Gets the parent of the Area2D, which is the card itself
                var newCard = area?.GetParent<Card>();

                if (newCard != null && newCard.ZIndex > highestZ)
                {
                    highestCard = newCard;
                    highestZ = newCard.ZIndex;
                }
            }

			// Only the topmost card under the mouse can be selected/deselected
            if (highestCard != card) return;

            // Proceed with select/deselect
            if (card == _selectedCard) 
				DeselectCard();
            else 
				SelectCard(card);
        }

		/// <summary>
        /// Makes the given card the currently selected card.
        /// Deselects any previously selected card, then make those scale back down,
        /// then applies selection visual effects to the new card.
        /// Finally, emits a signal to update other controllers.
        /// </summary>
        private void SelectCard(Card card)
        {
            if (_selectedCard == card) return;

			Card oldCard = _selectedCard; // May be null

            // Deselect previous card if there is any
            if (_selectedCard != null)
            {
                // Tween scale back to 1.0
                Tween tween = CreateTween();
                tween.TweenProperty(oldCard, "scale", Vector2.One, 0.05f);

                // Restore original ZIndex from the dictionary and priority
                oldCard.ZIndex = _cardSystem.OriginalZIndexes[oldCard];

                oldCard.UpdatePriority();
            }

			// Selects the new card
            _selectedCard = card;

			EmitSignal(SignalName.SelectionChanged, oldCard, card);

            // Apply selection visuals
            Tween selectTween = CreateTween();
            selectTween.TweenProperty(card, "scale", new Vector2(1.1f, 1.1f), 0.05f);

            card.ZIndex = 10;
            card.UpdatePriority();
        }

		/// <summary>
        /// Deselects the currently selected card, restoring its appearance.
        /// Emits a signal with oldCard = the deselected card and newCard(_selectedCard) = null.
        /// </summary>
        private void DeselectCard()
        {
            if (_selectedCard == null) return;
            Card oldSelected = _selectedCard;

            // Tween scale back to 1.0
            Tween tween = CreateTween();
            tween.TweenProperty(oldSelected, "scale", Vector2.One, CardSystem.TWEEN_DURATION);

            // Restore original ZIndex from the dictionary and priority
            oldSelected.ZIndex = _cardSystem.OriginalZIndexes[oldSelected];
            oldSelected.UpdatePriority();

            _selectedCard = null;

			EmitSignal(SignalName.SelectionChanged, oldSelected, _selectedCard);

        }
	}
}

