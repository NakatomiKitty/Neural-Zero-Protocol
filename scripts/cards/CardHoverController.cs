using Godot;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Cards
{
	/// <summary>
	/// Manages hover effects (scale tweening + ZIndex changes) for the cards.
	/// Only the topmost unselected card under the mouse gets the effect.
	/// </summary>
	public partial class CardHoverController : Node
	{
		private Dictionary<Card, Tween> _activeHoverTweens = new Dictionary<Card, Tween>();
        private HashSet<Card> _cardsUnderMouse = new HashSet<Card>();

		private CardSystem _cardSystem;
		private Card _selectedCard;

		private Card _currentHoveredCard;

		public override void _Ready() => _cardSystem = GetNode<CardSystem>("..");

		public void OnCardSelected(Card card)
		{
			// Selected card no longer gets the hover effect 
            _cardsUnderMouse.Remove(card);

            if (_currentHoveredCard == card)
			{
				if (_currentHoveredCard != null)

					// If the selected card was hovered, remove the hover effect immediately.
					ApplyHoverEffect(_currentHoveredCard, false);

				_currentHoveredCard = null;
			}

            // Stop any ongoing tweens on this card
            KillAndRemoveTween(card);

			_selectedCard = card;

			// Re‑evaluate the hover effect (the selected card is now ignored)
			UpdateHoverEffect();
		}

		public void OnCardDeselected(Card card)
		{
			_selectedCard = null;
			UpdateHoverEffect();
		}


		public void OnHoveredOverCard(Card card)
        {
            if (card == _selectedCard) return;   // Never hover the selected card

            _cardsUnderMouse.Add(card);
            UpdateHoverEffect();
        }

        public void OnHoveredOffCard(Card card)
        {
            if (card == _selectedCard) return; 

            _cardsUnderMouse.Remove(card);
            UpdateHoverEffect();
        }


        // Re‑evaluates which card (if there is any) should receive the hover effect.
        // The effect is given to the highest‑ZIndex card that is NOT selected.
		public void UpdateHoverEffect()
        {

            if (_cardsUnderMouse.Count == 0)
            {
                if (_currentHoveredCard != null)
                {
                    ApplyHoverEffect(_currentHoveredCard, false);
                    _currentHoveredCard = null;
                }
                return;
            }

            Card highestCard = null;
            int highestZ = int.MinValue;

            foreach (Card card in _cardsUnderMouse)
            {
				if (card == null) continue;
                if (card == _selectedCard) continue;
                if (card.ZIndex > highestZ)
                {
                    highestCard = card;
                    highestZ = card.ZIndex;
                }
            }

            if (highestCard != _currentHoveredCard)
            {
                if(_currentHoveredCard != null) ApplyHoverEffect(_currentHoveredCard, false);
                    
                if (highestCard != null) ApplyHoverEffect(highestCard, true);

                _currentHoveredCard = highestCard;
            }
            
        }

		// Kind of like a helper function! 
		// Removes and Kill any ongoing tween
		public void KillAndRemoveTween(Card card)
		{
			if (_activeHoverTweens.ContainsKey(card) 
            && _activeHoverTweens[card].IsRunning())
                _activeHoverTweens[card].Kill();

			_activeHoverTweens.Remove(card);
		}

		// Also a helper function!
		// Store a tween for later cleanup.
		public void AddTween(Card card, Tween tween)
		{
			KillAndRemoveTween(card);

			_activeHoverTweens[card] = tween;

            tween.Finished += () => _activeHoverTweens.Remove(card);
		}

		// The main brain! 
		// this actually applies (or removes) the hover effect: scale tweening + ZIndex change.
        private void ApplyHoverEffect(Card card, bool isHovered)
        {
            KillAndRemoveTween(card);

            Vector2 cardScale = isHovered ? new Vector2(1.1f, 1.1f) : Vector2.One;
            
            Tween tween = CreateTween();
            tween.TweenProperty(card, "scale", cardScale, 0.05f)
                .SetTrans(Tween.TransitionType.Linear)
                .SetEase(Tween.EaseType.InOut);

            AddTween(card, tween);

            if (isHovered)
            {
                card.ZIndex = 9; // Hovered goes above selected (10) and base (5)
                card.UpdatePriority();
            }
            else
            {
                card.ZIndex = _cardSystem.OriginalZIndexes[card];
                card.UpdatePriority(); 
            }
        }
		

	}
}

