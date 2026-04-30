using Godot;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Cards
{
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
			// Remove from hover set
            _cardsUnderMouse.Remove(card);

            if (_currentHoveredCard == card)
			{
				// Remove visual hover effect immediately
				if (_currentHoveredCard != null)
					ApplyHoverEffect(_currentHoveredCard, false);
				_currentHoveredCard = null;
			}

            // Kill any existing tween on new card
            KillAndRemoveTween(card);

			_selectedCard = card;
			UpdateHoverEffect();
		}

		public void OnCardDeselected(Card card)
		{
			if (!_cardsUnderMouse.Contains(card))
                _cardsUnderMouse.Add(card);

			_selectedCard = null;
            UpdateHoverEffect();
		}


		public void OnHoveredOverCard(Card card)
        {
            if (card == _selectedCard) return;   // skip if selected

            _cardsUnderMouse.Add(card);
            UpdateHoverEffect();
        }

        public void OnHoveredOffCard(Card card)
        {
            if (card == _selectedCard) return;   // skip if selected

            _cardsUnderMouse.Remove(card);
            UpdateHoverEffect();
        }

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

		public void KillAndRemoveTween(Card card)
		{
			if (_activeHoverTweens.ContainsKey(card) 
            && _activeHoverTweens[card].IsRunning())
                _activeHoverTweens[card].Kill();

			_activeHoverTweens.Remove(card);
		}

		public void AddTween(Card card, Tween tween)
		{
			KillAndRemoveTween(card);

			_activeHoverTweens[card] = tween;

            tween.Finished += () => _activeHoverTweens.Remove(card);
		}

        private void ApplyHoverEffect(Card card, bool isHovered)
        {
            // Kill any existing tween on this specific card
            KillAndRemoveTween(card);

            // Change Scale whether or not if it's being hovered
            Vector2 cardScale = isHovered ? new Vector2(1.1f, 1.1f) : Vector2.One;
            
            // Create a new tween just for this card
            Tween tween = CreateTween();
            tween.TweenProperty(card, "scale", cardScale, 0.05f)
                .SetTrans(Tween.TransitionType.Linear)
                .SetEase(Tween.EaseType.InOut);

            AddTween(card, tween);

            if (isHovered)
            {
                card.ZIndex = 11;
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

