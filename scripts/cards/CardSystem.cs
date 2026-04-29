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
            if (card == _selectedCard) DeselectCard(card);
        
            else SelectCard(card);
        }

        private void SelectCard(Card card)
        {
            if (_selectedCard == card) return;

            // Deselect previous card
            if (_selectedCard != null)
            {
                // Restore original ZIndex from CreateHandFromPath
                _selectedCard.ZIndex = _originalZIndexes[_selectedCard];

                // Kill any tween and reset scale to normal
                if (_activeHoverTweens.ContainsKey(_selectedCard) && _activeHoverTweens[_selectedCard].IsRunning())
                    _activeHoverTweens[_selectedCard].Kill();

                _activeHoverTweens.Remove(_selectedCard);

                _selectedCard.Scale = Vector2.One;
            }

            _selectedCard = card;

            // Kill any hover tween on the new card
            if (_activeHoverTweens.ContainsKey(card) && _activeHoverTweens[card].IsRunning())
                _activeHoverTweens[card].Kill();

            _activeHoverTweens.Remove(card);

            card.Scale = new Vector2(1.1f, 1.1f);
            card.ZIndex = 10;

            _currentHoveredCard = null;

            RefreshHoverUnderMouse(card);
        }

        private void DeselectCard(Card card)
        {
            if (_selectedCard == null) return;
    
            _selectedCard.ZIndex = _originalZIndexes[_selectedCard];

            if (_activeHoverTweens.ContainsKey(_selectedCard) && _activeHoverTweens[_selectedCard].IsRunning())
                _activeHoverTweens[_selectedCard].Kill();

            _activeHoverTweens.Remove(_selectedCard);

            _selectedCard.Scale = Vector2.One;
            _selectedCard = null;

            RefreshHoverUnderMouse(card);
        }

        private void OnHoveredOverCard(Card card)
        {
            if (card == _selectedCard) return;   // skip if selected

            ApplyHoverEffect(card, true);
        }

        private void OnHoveredOffCard(Card card)
        {
            if (card == _selectedCard) return;   // skip if selected

            ApplyHoverEffect(card, false);  
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
                card.ZIndex = 9;
            }
            else
            {
                card.ZIndex = _originalZIndexes[card];
            }
        }

        // private Card RaycastPickCard()
        // {
        //     var spaceState = GetWorld2D().DirectSpaceState;
        //     var parameters = new PhysicsPointQueryParameters2D
        //     {
        //         Position = GetGlobalMousePosition(),
        //         CollideWithAreas = true,
        //         CollisionMask = 1
        //     };

        //     var results = spaceState.IntersectPoint(parameters);
        //     if (results.Count == 0) return null;

        //     // Find the highest Z-index card among all cards
        //     Card highestCard = null;
        //     int highestZ = int.MinValue;

        //     foreach (var result in results)
        //     {
        //         var collider = (Node2D)result["collider"];
        //         var card = collider.GetParent() as Card;
        //         if (card != null && card.ZIndex > highestZ)
        //         {
        //             highestCard = card;
        //             highestZ = card.ZIndex; 
        //         }
        //     }
        //     return highestCard;
        // }

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
                _originalZIndexes[cards[i]] = cards[i].ZIndex;
            }
		}

        // Helper Functions
        private void RefreshHoverUnderMouse(Card card)
        {
            // Clear current hover state to force re‑evaluation
            if (_currentHoveredCard != null)
            {
                ApplyHoverEffect(_currentHoveredCard, false);
                _currentHoveredCard = null;
            }

            // Raycast to see if a card is under mouse
            if (card != null && card != _selectedCard)
            {
                _currentHoveredCard = card;
                ApplyHoverEffect(card, true);
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