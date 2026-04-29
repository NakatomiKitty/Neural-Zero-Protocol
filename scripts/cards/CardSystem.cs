using Godot;
using GodotUtilities;
using System;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Cards
{
    [Scene]
    public partial class CardSystem : Node2D
    {
        private static readonly PackedScene _card = GD.Load<PackedScene>("res://scenes/card.tscn");
        [Node] private Path2D _path2d;
        
        private Card _currentHoveredCard;
        private Card _selectedCard;

        private Dictionary<Card, int> _originalZIndexes = new Dictionary<Card, int>();
        private Dictionary<Card, Tween> _activeHoverTweens = new Dictionary<Card, Tween>();
        private HashSet<Card> _cardsUnderMouse = new HashSet<Card>();

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated) WireNodes();
        }

        public override void _Ready()
        {
            CreateHandFromPath(5);
        }

        private void OnCardClicked(Card card)
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

                if (card != null && card.ZIndex > highestZ)
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

            // Deselect previous card with tween
            if (_selectedCard != null)
            {
                // Kill any hover or selection tween on old card
                if (_activeHoverTweens.ContainsKey(_selectedCard) && _activeHoverTweens[_selectedCard].IsRunning())
                    _activeHoverTweens[_selectedCard].Kill();
                _activeHoverTweens.Remove(_selectedCard);
                
                // Tween scale back to 1.0
                Tween tween = CreateTween();
                tween.TweenProperty(_selectedCard, "scale", Vector2.One, 0.05f);
                _activeHoverTweens[_selectedCard] = tween;
                tween.Finished += () => _activeHoverTweens.Remove(_selectedCard);
                
                // Restore ZIndex
                _selectedCard.ZIndex = _originalZIndexes[_selectedCard];
                _selectedCard.UpdatePriority();
            }

            _selectedCard = card;

            // Remove from hover set
            _cardsUnderMouse.Remove(card);
            if (_currentHoveredCard == card)
                _currentHoveredCard = null;

            // Kill any existing tween on new card
            if (_activeHoverTweens.ContainsKey(card) && _activeHoverTweens[card].IsRunning())
                _activeHoverTweens[card].Kill();
            _activeHoverTweens.Remove(card);

            // Tween scale to 1.1
            Tween selectTween = CreateTween();
            selectTween.TweenProperty(card, "scale", new Vector2(1.1f, 1.1f), 0.05f);
            _activeHoverTweens[card] = selectTween;
            selectTween.Finished += () => _activeHoverTweens.Remove(card);

            // Set ZIndex and priority
            card.ZIndex = 10;
            card.UpdatePriority();

            UpdateHoverEffect();
        }

        private void DeselectCard()
        {
            if (_selectedCard == null) return;

            Card oldSelected = _selectedCard;

            // Kill any tween on old selected
            if (_activeHoverTweens.ContainsKey(oldSelected) && _activeHoverTweens[oldSelected].IsRunning())
                _activeHoverTweens[oldSelected].Kill();
            _activeHoverTweens.Remove(oldSelected);

            // Tween scale back to 1.0
            Tween tween = CreateTween();
            tween.TweenProperty(oldSelected, "scale", Vector2.One, 0.05f);
            _activeHoverTweens[oldSelected] = tween;
            tween.Finished += () => _activeHoverTweens.Remove(oldSelected);

            // Restore ZIndex
            oldSelected.ZIndex = _originalZIndexes[oldSelected];
            oldSelected.UpdatePriority();

            _selectedCard = null;

            // Re-add to under-mouse set
            if (!_cardsUnderMouse.Contains(oldSelected))
                _cardsUnderMouse.Add(oldSelected);

            UpdateHoverEffect();
        }

        private void OnHoveredOverCard(Card card)
        {
            if (card == _selectedCard) return;   // skip if selected

            _cardsUnderMouse.Add(card);
            UpdateHoverEffect();
        }

        private void OnHoveredOffCard(Card card)
        {
            if (card == _selectedCard) return;   // skip if selected

            _cardsUnderMouse.Remove(card);
            UpdateHoverEffect();
        }

        private void UpdateHoverEffect()
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

        private void ApplyHoverEffect(Card card, bool isHovered)
        {
            // Kill any existing tween on this specific card
            if (_activeHoverTweens.ContainsKey(card) && _activeHoverTweens[card].IsRunning())
                _activeHoverTweens[card].Kill();

            // Remove the entry if it exists
            _activeHoverTweens.Remove(card);

            // Change Scale whether or not if it's being hovered
            Vector2 cardScale = isHovered ? new Vector2(1.1f, 1.1f) : Vector2.One;
            
            // Create a new tween just for this card
            Tween tween = CreateTween();
            tween.TweenProperty(card, "scale", cardScale, 0.05f)
                .SetTrans(Tween.TransitionType.Linear)
                .SetEase(Tween.EaseType.InOut);

            _activeHoverTweens[card] = tween;

            tween.Finished += () => _activeHoverTweens.Remove(card);

            if (isHovered)
            {
                card.ZIndex = 11;
                card.UpdatePriority();
            }
            else
            {
                card.ZIndex = _originalZIndexes[card];
                card.UpdatePriority(); 
            }
        }

        private void CreateHandFromPath(int cardCount)
		{
			Curve2D curve = _path2d.Curve;
			float totalLength = curve.GetBakedLength();

            List<Card> cards = new List<Card>();

            float spacing = 105f; 
            float centerOffset = totalLength / 2f;

			for (int i = 0; i < cardCount; i++)
			{
				float indexOffset = i - (cardCount - 1) / 2f;

                float offset = centerOffset + indexOffset * spacing;

                offset = Mathf.Clamp(offset, 0, totalLength);

				Vector2 position = curve.SampleBaked(offset);

				Card card = _card.Instantiate<Card>();
				AddChild(card);

				card.Position = position;

                float maxRotation = Mathf.DegToRad(15f);
                float normalized = indexOffset / ((cardCount - 1) / 2f);

                card.Rotation = normalized * maxRotation;

                ConnectCard(card);
                cards.Add(card);
			}

            // Assign Z indexes: Example. 3, 4, 5, 4, 3
            int centerIndex = cardCount / 2;          
            int maxZ = 5;                             
            
            for (int i = 0; i < cards.Count; i++)
            {
                int distanceFromCenter = Math.Abs(i - centerIndex);
                cards[i].ZIndex = maxZ - distanceFromCenter;
                cards[i].UpdatePriority();
                _originalZIndexes[cards[i]] = cards[i].ZIndex;
            }
		}

        private void ConnectCard(Card card)
		{
			card.Hovered += OnHoveredOverCard;
    		card.NotHovered += OnHoveredOffCard;
            card.Clicked += OnCardClicked;
		}
    }
}