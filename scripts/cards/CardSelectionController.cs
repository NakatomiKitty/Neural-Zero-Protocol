using System.Linq;
using Godot;
using Godot.Collections;

namespace NeuralZeroProtocol.Scripts.Cards
{
    /// <summary>
    /// Handles card selection/deselection logic, including visual feedback (scale, ZIndex, priority).
    /// When selection changes, emits a signal to notify other controllers.
    /// </summary>
    public partial class CardSelectionController : Node2D
    {
        private const float SELECTION_TWEEN_DURATION = 0.2f;
        private const float SWAP_TWEEN_DURATION = 0.15f;

        [Signal] public delegate void SelectionChangedEventHandler(Card oldSelectedCard, Card newSelectedCard);
        [Signal] public delegate void GetHighlightedCardEventHandler(Card card);
        [Signal] public delegate void SwappingStateChangedEventHandler(bool isSwapping);
        public Dictionary<Card, Tween> ActivePositionTweens = new();
        public Array<Card> CardHand;
        public Card CenterCard;
        public bool IsSwapping;
        public bool MouseModeActive;
        
        private Tween _swapTweenClicked, _swapTweenCenter;
        private CardSystem _cardSystem;
        private Card _mouseSelectedCard; // Used for selecting with mouse
        private Card _keyboardHighlightedCard; // Used for selecting with keyboard

        public override void _Ready() => _cardSystem = GetNode<CardSystem>("..");

        public void SetCardHand(Array<Node> cardHand)
        {
            CardHand = [.. cardHand.OfType<Card>()];

            GD.Print(CardHand);
        } 

        public void SetCenterCard(Card centerCard)
        {
            _keyboardHighlightedCard = centerCard;

            CenterCard = centerCard;

            if (_mouseSelectedCard == null) SelectCard(centerCard);

            EmitSignal(SignalName.GetHighlightedCard, CenterCard);
        }
        
        public void MoveLeft()
        {
            if (CardHand == null || CardHand.Count == 0) return;

            int centerIndex = CardHand.IndexOf(_keyboardHighlightedCard);

            int toLeftCardIndex = centerIndex - 1;

            if(toLeftCardIndex < 0) toLeftCardIndex = CardHand.Count - 1;

            _keyboardHighlightedCard = CardHand[toLeftCardIndex];

            GD.Print(_keyboardHighlightedCard.Name);

            EmitSignal(SignalName.GetHighlightedCard, _keyboardHighlightedCard);
        }

        public void MoveRight()
        {
            if (CardHand == null || CardHand.Count == 0) return;

            int centerIndex = CardHand.IndexOf(_keyboardHighlightedCard);

            int toRightCardIndex = centerIndex + 1;

            if(toRightCardIndex >= CardHand.Count) toRightCardIndex = 0;

            _keyboardHighlightedCard = CardHand[toRightCardIndex];

            GD.Print(_keyboardHighlightedCard.Name);

            EmitSignal(SignalName.GetHighlightedCard, _keyboardHighlightedCard);
        }

        public void ConfirmCard()
        {
            if (CardHand == null || CardHand.Count == 0) return;

            if (MouseModeActive) return; // Mouse selection overrides hotkeys
            if (IsSwapping) return; // Already swapping, ignore
            if (_keyboardHighlightedCard == null) return; // No valid highlight

            // If the highlighted card is already the center, do nothing
            if (_keyboardHighlightedCard == CenterCard) return;

            // Trigger the same swap logic as if the card was clicked
            SwapWithCenter(_keyboardHighlightedCard);
        }

        /// <summary>
		/// This is a hefty one, Called when any card is clicked, Then:
        /// 1. Finds the topmost card under the mouse (prevents clicks when cards overlap).
        /// 2. If clicked card is not the center, attempt to swap it with the center.
        /// 3. Otherwise, toggle selection (only if it's already not selected).
        /// </summary>
        public void OnCardClicked(Card clickedCard)
        {
            if (!IsTopmostCard(clickedCard)) return;
            
            // Swap with center card
            if (clickedCard != CenterCard)
            {
                if (IsSwapping) return; // Already swapping, ignore additional clicks
                SwapWithCenter(clickedCard);
                return;
            }
            
            // Clicked the center card – select/deselect it
            if (clickedCard != _mouseSelectedCard) SelectCard(clickedCard);
        }
        
        // Makes the given card the selected one. Deselects any previous selection.
        public void SelectCard(Card card)
        {
            if (_mouseSelectedCard == card) return;
            
            // Deselect previous card if there is one
            if (_mouseSelectedCard != null) ApplyVisualState(_mouseSelectedCard, false);
            
            var oldCard = _mouseSelectedCard;
            _mouseSelectedCard = card;

            MouseModeActive = true;
            EmitSignal(SignalName.SelectionChanged, oldCard, card);
            
            // Apply visual effects for the new selected card
            ApplyVisualState(card, true);
        }
        
        // Deselects the currently selected card and emits a signal with newCard = null.
        private void DeselectCard()
        {
            if (_mouseSelectedCard == null) return;

            var oldSelected = _mouseSelectedCard;

            ApplyVisualState(oldSelected, false);

            _mouseSelectedCard = null;

            MouseModeActive = false;

            EmitSignal(SignalName.GetHighlightedCard, CenterCard);

            EmitSignal(SignalName.SelectionChanged, oldSelected, _mouseSelectedCard);
        }
        
        /// <summary>
        /// Applies the visual effects(scale, position offset, ZIndex) for either selected OR deselected.
        /// Uses a single tween with parallel animations to avoid conflicts.
        /// </summary>
        /// <param name="card"> The card to modify.</param>
        /// <param name="selected"> If true, scale card up, lift it up slightly, and raise it's ZIndex,
        /// if false, restore original.</param>
        private void ApplyVisualState(Card card, bool selected)
        {
            KillPositionTween(card);

            var tween = CreateTween();
            var basePosition = _cardSystem.CardBasePositions[card];
            var targetScale = selected ? Vector2.One * 1.15f : Vector2.One;
            var targetPosition = selected ? basePosition + Vector2.Down * -20 : basePosition;
            var duration = SELECTION_TWEEN_DURATION;
            
            tween.Parallel()
                .TweenProperty(card, "scale", targetScale, duration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            tween.Parallel()
                .TweenProperty(card, "position", targetPosition, duration)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);

            ActivePositionTweens[card] = tween;
            
            card.ZIndex = selected ? CardSystem.SELECTED_Z : _cardSystem.OriginalZIndexes[card];
            
            card.UpdatePriority();
        }
        
        // Performs a physics point query to find the card with the highest ZIndex under the mouse.
        // Returns true if the given clickedCard is indeed the topmost one.
        private bool IsTopmostCard(Card clickedCard)
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
            var highestZ = int.MinValue;
            
            foreach (var result in results)
            {
                if (result["collider"].As<Area2D>()?.GetParent<Card>() is Card card && card.ZIndex > highestZ)
                {
                    highestCard = card;
                    highestZ = card.ZIndex;
                }
            }
            return highestCard == clickedCard;
        }
        
        /// <summary>
        /// The main appeal! Swaps the clicked card with the current center card.
        /// Then tweens the positions and rotations, then swaps their base positions and ZIndex in the CardSystem.
        /// Then, finally selects the new center card.
        /// </summary>
        private void SwapWithCenter(Card clickedCard)
        {
            if (IsSwapping) return;

            _swapTweenClicked?.Kill();
            _swapTweenCenter?.Kill();

            var oldCenter = CenterCard;
            var clickedBase = _cardSystem.CardBasePositions[clickedCard];
            var centerBase = _cardSystem.CardBasePositions[oldCenter];
            
            KillPositionTween(clickedCard);
            KillPositionTween(oldCenter);
            
            IsSwapping = true;

            EmitSignal(SignalName.SwappingStateChanged, IsSwapping);

            // Tween shit
            _swapTweenClicked = CreateTween();
            _swapTweenCenter = CreateTween();
            
            _swapTweenClicked.TweenProperty(clickedCard, "position", centerBase, SWAP_TWEEN_DURATION)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.InOut);
            
            _swapTweenClicked.Parallel().TweenProperty(clickedCard, "rotation", oldCenter.Rotation, SWAP_TWEEN_DURATION);
            
            _swapTweenCenter.TweenProperty(oldCenter, "position", clickedBase, SWAP_TWEEN_DURATION)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.InOut);

            _swapTweenCenter.Parallel().TweenProperty(oldCenter, "rotation", clickedCard.Rotation, SWAP_TWEEN_DURATION);
            
            _swapTweenClicked.Finished += () =>
            {
                if (!IsSwapping) return;

                // Swap base positions
                (_cardSystem.CardBasePositions[clickedCard], _cardSystem.CardBasePositions[oldCenter]) = (centerBase, clickedBase);
                
                // Swap Z indexes
                (_cardSystem.OriginalZIndexes[clickedCard], _cardSystem.OriginalZIndexes[oldCenter]) = 
                    (_cardSystem.OriginalZIndexes[oldCenter], _cardSystem.OriginalZIndexes[clickedCard]);

                int clickedIndex = CardHand.IndexOf(clickedCard);
                int centerIndex = CardHand.IndexOf(oldCenter);
                if (clickedIndex != -1 && centerIndex != -1)
                {
                    CardHand[clickedIndex] = oldCenter;
                    CardHand[centerIndex] = clickedCard;
                }
                
                // First, deselect the current centerCard
                DeselectCard();
                // Then select the new centerCard
                SelectCard(clickedCard);
                
                // Don't forget to update the _centerCard value to the new _centerCard!
                CenterCard = clickedCard;

                EmitSignal(SignalName.GetHighlightedCard, CenterCard);

                clickedCard.UpdatePriority();
                oldCenter.UpdatePriority();
                
                IsSwapping = false;

                EmitSignal(SignalName.SwappingStateChanged, IsSwapping);

                _swapTweenClicked = _swapTweenCenter = null;
            };
        }
        
        // Safely kills an active position tween for a card and removes it from the dictionary.
        // Just a helper function
        private void KillPositionTween(Card card)
        {
            if (ActivePositionTweens.TryGetValue(card, out var tween) && tween.IsRunning())
                tween.Kill();
            ActivePositionTweens.Remove(card);
        }
    }
}

