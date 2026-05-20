using System.Linq;
using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Ui;

namespace NeuralZeroProtocol.Scripts.Cards;

/// <summary>
/// Handles card selection/deselection logic, including visual feedback (scale, ZIndex, priority).
/// When selection changes, emits a signal to notify other controllers.
/// </summary>

public enum CardSelectionState
{
    Idle,
    KeyboardMode,
    MouseMode,
    SwapTheCards
}

public partial class CardSelectionController : Node2D
{
    private const float SelectionTweenDuration = 0.2f;
    private const float SwapTweenDuration = 0.15f;

    [Signal] public delegate void KeyboardModeDeactivatedEventHandler();
    [Signal] public delegate void SelectionChangedEventHandler(Card oldSelectedCard, Card newSelectedCard);
    [Signal] public delegate void GetKeyboardHoveredCardEventHandler(Card card);
    [Signal] public delegate void SwappingStateChangedEventHandler(bool isSwapping);
    
    private CardSelectionState _state;
    private CardSystem _cardSystem;
    private Tween _swapCardTween;
    private Card _selectedCard; // Used for selecting with mouse
    private Card _keyboardHoveredCard; // Used for selecting with keyboard
    
    public Dictionary<Card, Tween> ActivePositionTweens = new();
    public Array<Card> CardHand { get; private set; }
    public Card CenterCard;

    public override void _Ready() => _cardSystem = GetNode<CardSystem>("..");
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion && _state == CardSelectionState.KeyboardMode)
        {
            DeactivateKeyboardMode();
        }
        
        if (@event is InputEventMouseButton mouseButton && 
            mouseButton.ButtonIndex == MouseButton.Left && 
            mouseButton.Pressed)
        {
            // If we're in a state that allows deselection (e.g., not swapping)
            if (_state != CardSelectionState.SwapTheCards)
            {
                // Check if any card was clicked
                if (!IsAnyCardUnderMouse())
                {
                    // Clicked on empty space → deselect and return to Idle
                    DeactivateKeyboardMode();
                    ChangeState(CardSelectionState.Idle);
                }
            }
        }
    }
    
    public void SetCardHand(Array<Node> cardHand) => CardHand = [.. cardHand.OfType<Card>()];
    
    public void SetCenterCard(Card centerCard)
    {
        _keyboardHoveredCard = centerCard;
        CenterCard = centerCard;
        if (_selectedCard == null) SelectCard(centerCard);
        EmitSignal(SignalName.GetKeyboardHoveredCard, CenterCard);
        ChangeState(CardSelectionState.Idle);
    }

    private void ChangeState(CardSelectionState newState)
    {
        _state = newState;
        switch (_state)
        {
            case CardSelectionState.SwapTheCards:
                EmitSignal(SignalName.SwappingStateChanged, true);
                break;
            default:
                if (_state != CardSelectionState.SwapTheCards)
                    EmitSignal(SignalName.SwappingStateChanged, false);
                break;
        }
    }

    #region Keyboard Mode
    public void GetUiInput(UiSelection uiSelection)
    {
        if (_state == CardSelectionState.SwapTheCards) return;

        switch (uiSelection)
        {
            case UiSelection.Left:
            case UiSelection.Right:
            {
                if (_state != CardSelectionState.KeyboardMode)
                    ActivateKeyboardMode();
                if (uiSelection == UiSelection.Left)
                    KeyboardMoveLeft();
                else
                    KeyboardMoveRight();
                return;
            }
            case UiSelection.Confirm when _state == CardSelectionState.KeyboardMode:
                ConfirmCard();
                break;
            case UiSelection.Cancel:
                DeactivateKeyboardMode();
                ChangeState(CardSelectionState.Idle);
                break;
        }
    }
    
    public void KeyboardMoveLeft()
    {
        if (CardHand == null || CardHand.Count == 0) return;

        int centerIndex = CardHand.IndexOf(_keyboardHoveredCard);

        int toLeftCardIndex = centerIndex - 1;

        if (toLeftCardIndex < 0) toLeftCardIndex = CardHand.Count - 1;

        _keyboardHoveredCard = CardHand[toLeftCardIndex];

        EmitSignal(SignalName.GetKeyboardHoveredCard, _keyboardHoveredCard);
    }
    
    public void KeyboardMoveRight()
    {
        if (CardHand == null || CardHand.Count == 0) return;

        int centerIndex = CardHand.IndexOf(_keyboardHoveredCard);

        int toRightCardIndex = centerIndex + 1;

        if(toRightCardIndex >= CardHand.Count) toRightCardIndex = 0;

        _keyboardHoveredCard = CardHand[toRightCardIndex];

        EmitSignal(SignalName.GetKeyboardHoveredCard, _keyboardHoveredCard);
    }

    public void ConfirmCard()
    {
        if (_state != CardSelectionState.KeyboardMode) return;
        
        if (CardHand == null || CardHand.Count == 0) return;
        
        if (_keyboardHoveredCard == null) return; 
        
        if (_keyboardHoveredCard == CenterCard) return;

        // Trigger the same swap logic as if the card was clicked
        SwapWithCenter(_keyboardHoveredCard);
    }
    
    #endregion
    
    #region Mouse Mode
    public void OnCardClicked(Card clickedCard)
    {
        if (_state == CardSelectionState.SwapTheCards) return;
        if (!IsTopmostCard(clickedCard)) return;
        
        // Swap with center card
        if (clickedCard != CenterCard)
        {
            SwapWithCenter(clickedCard);
            return;
        }
        
        if (clickedCard != _selectedCard) SelectCard(clickedCard);
    }
    
    // Makes the given card the selected one. Deselects any previous selection.
    public void SelectCard(Card card)
    {
        if (_selectedCard == card) return;
        
        // Deselect previous card if there is one
        if (_selectedCard != null) ApplyVisualState(_selectedCard, false);
        
        Card oldCard = _selectedCard;
        _selectedCard = card;

        ChangeState(CardSelectionState.MouseMode);
        EmitSignal(SignalName.SelectionChanged, oldCard, card);
        
        // Apply visual effects for the new selected card
        ApplyVisualState(card, true);
    }
    
    #endregion
    
    private void DeselectCard()
    {
        if (_selectedCard == null) return;

        Card oldSelected = _selectedCard;

        ApplyVisualState(oldSelected, false);

        _selectedCard = null;
        
        EmitSignal(SignalName.GetKeyboardHoveredCard, CenterCard);

        EmitSignal(SignalName.SelectionChanged, oldSelected, _selectedCard);
    }
    
    private void SwapWithCenter(Card clickedCard)
    {
        if (_state == CardSelectionState.SwapTheCards) return;
        ChangeState(CardSelectionState.SwapTheCards);

        _swapCardTween?.Kill();

        Card oldCenter = CenterCard;
        Vector2 clickedBase = _cardSystem.CardBasePositions[clickedCard];
        Vector2 centerBase = _cardSystem.CardBasePositions[oldCenter];
        
        KillPositionTween(clickedCard);
        KillPositionTween(oldCenter);

        // Tween shit
        
        _swapCardTween = CreateTween();
        
        _swapCardTween.Parallel().TweenProperty(clickedCard, "position", centerBase, SwapTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.InOut);;
        _swapCardTween.Parallel().TweenProperty(clickedCard, "rotation", oldCenter.Rotation, SwapTweenDuration);
        
        _swapCardTween.Parallel().TweenProperty(oldCenter, "position", clickedBase, SwapTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.InOut);;
        
        _swapCardTween.Parallel().TweenProperty(oldCenter, "rotation", clickedCard.Rotation, SwapTweenDuration);
        
        if (_swapCardTween != null)
            _swapCardTween.Finished += () =>
            {
                if (_state != CardSelectionState.SwapTheCards) return;

                // Swap base positions
                (_cardSystem.CardBasePositions[clickedCard], _cardSystem.CardBasePositions[oldCenter]) =
                    (centerBase, clickedBase);

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

                EmitSignal(SignalName.GetKeyboardHoveredCard, CenterCard);

                clickedCard.UpdatePriority();
                oldCenter.UpdatePriority();

                ChangeState(CardSelectionState.MouseMode);
            };
    }

    #region Helper Functions
    
    // Safely kills an active position tween for a card and removes it from the dictionary.
    // Just a helper function
    /// <summary>
    /// Applies the visual effects(scale, position offset, ZIndex) for either selected OR deselected.
    /// Uses a single tween with parallel animations to avoid conflicts.
    /// </summary>
    private void ApplyVisualState(Card card, bool selected)
    {
        KillPositionTween(card);

        Tween tween = CreateTween();
        Vector2 basePosition = _cardSystem.CardBasePositions[card];
        Vector2 targetScale = selected ? Vector2.One * 1.15f : Vector2.One;
        Vector2 targetPosition = selected ? basePosition + Vector2.Down * -20 : basePosition;

        tween.Parallel()
            .TweenProperty(card, "scale", targetScale, SelectionTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
        
        tween.Parallel()
            .TweenProperty(card, "position", targetPosition, SelectionTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        ActivePositionTweens[card] = tween;
        
        card.ZIndex = selected ? CardSystem.SelectedZ : _cardSystem.OriginalZIndexes[card];
        
        card.UpdatePriority();
    }
    
    private void ActivateKeyboardMode()
    {
        // Start keyboard highlight from the current center card
        _keyboardHoveredCard = CenterCard;
        EmitSignal(SignalName.GetKeyboardHoveredCard, _keyboardHoveredCard);
        ChangeState(CardSelectionState.KeyboardMode);
    }
    
    private void DeactivateKeyboardMode()
    {
        if (_state != CardSelectionState.KeyboardMode) return;
        
        EmitSignal(SignalName.KeyboardModeDeactivated);
        
        ChangeState(CardSelectionState.Idle);
        
        _keyboardHoveredCard = null;
    }
    
    // Performs a physics point query to find the card with the highest ZIndex under the mouse.
    // Returns true if the given clickedCard is indeed the topmost one.
    private bool IsTopmostCard(Card clickedCard)
    {
        Card highestCard = null;
        int highestZ = int.MinValue;
        
        foreach (Dictionary result in MouseQuery())
        {
            if (result["collider"].As<Area2D>()?.GetParent<Card>() is Card card && card.ZIndex > highestZ)
            {
                highestCard = card;
                highestZ = card.ZIndex;
            }
        }
        return highestCard == clickedCard;
    }
    
    private bool IsAnyCardUnderMouse()
    {
        foreach (var result in MouseQuery())
        {
            if (result["collider"].As<Area2D>()?.GetParent<Card>() is Card)
                return true;
        }
        return false;
    }

    private Array<Dictionary> MouseQuery()
    {
        Vector2 mousePos = GetGlobalMousePosition();
        PhysicsDirectSpaceState2D space = GetWorld2D().DirectSpaceState;
        PhysicsPointQueryParameters2D query = new()
        {
            Position = mousePos,
            CollideWithAreas = true,
            CollisionMask = 1
        };

        Array<Dictionary> results = space.IntersectPoint(query);
        return results;
    }
    
    private void KillPositionTween(Card card)
    {
        if (ActivePositionTweens.TryGetValue(card, out Tween tween) && tween.IsRunning())
            tween.Kill();
        ActivePositionTweens.Remove(card);
    }
    #endregion
}


