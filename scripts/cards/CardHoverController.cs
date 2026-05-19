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
        private const float HoverTweenDuration = 0.4f;

        private readonly Dictionary<Card, Tween> _activeHoverTweens = new();
        private readonly HashSet<Card> _cardsUnderMouse = new();

		private CardSystem _cardSystem;
		private Card _selectedCard;

		private Card _mouseHoveredCard; // Used for selecting with mouse 
        private Card _forcedHighlightedCard; // Used for selecting with keyboard

        private bool _isSwapping;
        
		public override void _Ready() => _cardSystem = GetNode<CardSystem>("..");

		private Card GetForcedHighlightedCard() => _forcedHighlightedCard;
        
        public void OnSwappingStateChanged(bool isSwapping) => _isSwapping = isSwapping;

		public void OnCardSelected(Card card)
		{
			// Selected card no longer gets the hover effect 
            _cardsUnderMouse.Remove(card);

            if (_mouseHoveredCard == card)
			{
				if (_mouseHoveredCard != null)

					// If the selected card was hovered, remove the hover effect immediately.
					ApplyHoverEffect(_mouseHoveredCard, false);

				_mouseHoveredCard = null;
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
            if (_isSwapping) return;

            if (card == _forcedHighlightedCard) return;

            if (card == _selectedCard) return;   // Never hover the selected card

            _cardsUnderMouse.Add(card);
            UpdateHoverEffect();
        }

        public void OnHoveredOffCard(Card card)
        {
            if (_isSwapping) return;

            if (card == _forcedHighlightedCard) return;

            if (card == _selectedCard) return; 

            _cardsUnderMouse.Remove(card);

            UpdateHoverEffect();
        }

        public void ForceHighlight(Card card)
        {
            ClearForcedHighlight();

            if (card == _forcedHighlightedCard) return;

            if (card == _selectedCard) return;

           _forcedHighlightedCard = card;

           ApplyHoverEffect(_forcedHighlightedCard, true);
        }

        private void ClearForcedHighlight()
        {
            if (_forcedHighlightedCard == null) return;

            Card highlightedCard = _forcedHighlightedCard;

            _forcedHighlightedCard = null;

            UpdateHoverEffect();

            ApplyHoverEffect(highlightedCard, false);
        }
        // Re‑evaluates which card (if there is any) should receive the hover effect.
        // The effect is given to the highest‑ZIndex card that is NOT selected.
        private void UpdateHoverEffect()
        {
            if (_isSwapping) return;

            if (_forcedHighlightedCard != null) return;

            if (_cardsUnderMouse.Count == 0)
            {
                if (_mouseHoveredCard != null)
                {
                    ApplyHoverEffect(_mouseHoveredCard, false);
                    _mouseHoveredCard = null;
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

            if (highestCard != _mouseHoveredCard)
            {
                if(_mouseHoveredCard != null) ApplyHoverEffect(_mouseHoveredCard, false);
                    
                if (highestCard != null) ApplyHoverEffect(highestCard, true);

                _mouseHoveredCard = highestCard;
            }
        }

		// Kind of like a helper function! 
		// Removes and Kill any ongoing tween
		private void KillAndRemoveTween(Card card)
		{
			if (_activeHoverTweens.ContainsKey(card) && _activeHoverTweens[card].IsRunning())
                _activeHoverTweens[card].Kill();

			_activeHoverTweens.Remove(card);
		}

		// Also a helper function!
		// Store a tween for later cleanup.
		private void AddTween(Card card, Tween tween)
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
            
            tween.TweenProperty(card, "scale", cardScale, HoverTweenDuration)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out);

            AddTween(card, tween);

            card.ZIndex = isHovered ? CardSystem.HoverZ : // Hovered goes above base (5)
	            _cardSystem.OriginalZIndexes[card];

            card.UpdatePriority();
        }
	}
}

