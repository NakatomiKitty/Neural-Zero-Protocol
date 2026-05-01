using Godot;
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
        
        private readonly Dictionary<Card, Tween> _activePositionTweens = new();
        private bool _isSwapping;
        private Tween _swapTweenClicked, _swapTweenCenter;
        
        private CardSystem _cardSystem;
        private Card _centerCard;
        private Card _selectedCard;

        public override void _Ready() => _cardSystem = GetNode<CardSystem>("..");

        public void SetCenterCard(Card centerCard)
        {
            _centerCard = centerCard;
            if (_selectedCard == null) SelectCard(centerCard);
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
            if (clickedCard != _centerCard)
            {
                if (_isSwapping) return; // Already swapping, ignore additional clicks
                SwapWithCenter(clickedCard);
                return;
            }
            
            // Clicked the center card – select/deselect it
            if (clickedCard != _selectedCard) SelectCard(clickedCard);
        }
        
        // Makes the given card the selected one. Deselects any previous selection.
        public void SelectCard(Card card)
        {
            if (_selectedCard == card) return;
            
            // Deselect previous card if there is one
            if (_selectedCard != null) ApplyVisualState(_selectedCard, false);
            
            var oldCard = _selectedCard;
            _selectedCard = card;

            EmitSignal(SignalName.SelectionChanged, oldCard, card);
            
            // Apply visual effects for the new selected card
            ApplyVisualState(card, true);
        }
        
        // Deselects the currently selected card and emits a signal with newCard = null.
        private void DeselectCard()
        {
            if (_selectedCard == null) return;

            var oldSelected = _selectedCard;

            ApplyVisualState(oldSelected, false);

            _selectedCard = null;

            EmitSignal(SignalName.SelectionChanged, oldSelected, _selectedCard);
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
            var targetPosition = selected ? basePosition + Vector2.Down * -20 : basePosition; // up = -Y
            var duration = CardSystem.TWEEN_DURATION;
            
            tween.Parallel().TweenProperty(card, "scale", targetScale, duration);
            tween.Parallel().TweenProperty(card, "position", targetPosition, duration);

            _activePositionTweens[card] = tween;
            
            card.ZIndex = selected ? 10 : _cardSystem.OriginalZIndexes[card];
            
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
            _swapTweenClicked?.Kill();
            _swapTweenCenter?.Kill();
            
            var oldCenter = _centerCard;
            var clickedBase = _cardSystem.CardBasePositions[clickedCard];
            var centerBase = _cardSystem.CardBasePositions[oldCenter];
            
            KillPositionTween(clickedCard);
            KillPositionTween(oldCenter);
            
            _isSwapping = true;
            
            // Tween shit
            _swapTweenClicked = CreateTween();
            _swapTweenCenter = CreateTween();
            
            _swapTweenClicked.TweenProperty(clickedCard, "position", centerBase, 0.1f);
            _swapTweenClicked.Parallel().TweenProperty(clickedCard, "rotation", oldCenter.Rotation, 0.1f);
            
            _swapTweenCenter.TweenProperty(oldCenter, "position", clickedBase, 0.1f);
            _swapTweenCenter.Parallel().TweenProperty(oldCenter, "rotation", clickedCard.Rotation, 0.1f);
            
            _swapTweenClicked.Finished += () =>
            {
                if (!_isSwapping) return;
                
                // Swap base positions
                (_cardSystem.CardBasePositions[clickedCard], _cardSystem.CardBasePositions[oldCenter]) = (centerBase, clickedBase);
                
                // Swap Z indexes
                (_cardSystem.OriginalZIndexes[clickedCard], _cardSystem.OriginalZIndexes[oldCenter]) = 
                    (_cardSystem.OriginalZIndexes[oldCenter], _cardSystem.OriginalZIndexes[clickedCard]);
                
                // First, deselect the current centerCard
                DeselectCard();
                // Then select the new centerCard
                SelectCard(clickedCard);
                
                // Don't forget to update the _centerCard value to the new _centerCard!
                _centerCard = clickedCard;

                clickedCard.UpdatePriority();
                oldCenter.UpdatePriority();
                
                _isSwapping = false;
                _swapTweenClicked = _swapTweenCenter = null;
            };
        }
        
        // Safely kills an active position tween for a card and removes it from the dictionary.
        // Just a helper function
        private void KillPositionTween(Card card)
        {
            if (_activePositionTweens.TryGetValue(card, out var tween) && tween.IsRunning())
                tween.Kill();
            _activePositionTweens.Remove(card);
        }
    }
}

