using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace NeuralZeroProtocol.Scripts.Cards;

/// <summary>
/// Handles card selection/deselection logic, including visual feedback (scale, ZIndex, priority).
/// When selection changes, emits a signal to notify other controllers.
/// </summary>

public sealed partial class CardSelectionController : Node2D
{
    private const float MouseMovementThreshold = 5.0f;
    
    private const float SelectionTweenDuration = 0.15f;
    private const float SwapTweenDuration = 0.15f;
    
    private const float SelectedCardSizeMultiplier = 1.15f;
    private const int SelectedCardVerticalOffset = -20;
    
    public event Action<Card, Card> SelectionChanged; // Card oldSelectedCard, Card newSelectedCard
    public event Action<Card> KeyboardHoveredCardChanged;
    public event Action<Card> CurrentSelectedCardChanged; // Card currentSelectedCard
    public event Action<bool> SwappingStateChanged; // bool isSwapping
    public event Action KeyboardModeDeactivated;
    public event Action KeyboardModeActivated;
    public event Action<bool> KeyboardModeCancelled; // bool isCancelled
    
    private Dictionary<Card, Tween> _activePositionTweens = new();
    
    private enum CardSelectionState { Idle, KeyboardMode, MouseMode, SwapTheCards }
    private CardSelectionState _currentState;
    
    private Tween _swapCardTween;
        
    private Card _selectedCard; // Used for selecting with mouse
    private Card _keyboardHoveredCard; // Used for selecting with keyboard
    
    private bool _isInActionMenu;
    
    private Vector2 _activationMousePos;
    private bool _ignoreMouseMotion;
    
    public Dictionary<Card, Vector2> CardBasePositions = new();
    public Dictionary<Card, int> OriginalZIndexes = new();

    private Array<Card> _cardHand = new();
    private Card _centerCard;

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            _swapCardTween?.Kill();
            
            foreach (Tween tween in _activePositionTweens.Values) tween?.Kill();
            
            _activePositionTweens.Clear();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseMotion motion when _currentState == CardSelectionState.KeyboardMode:
                if (!_ignoreMouseMotion && _activationMousePos.DistanceTo(motion.GlobalPosition) < MouseMovementThreshold)
                {
                    return;   
                }
                _ignoreMouseMotion = true;
                
                _isInActionMenu = true;
                KeyboardModeCancelled?.Invoke(true);
                DeactivateKeyboardMode();
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }:
            {
                // If we're in a state that allows deselection (e.g., not swapping)
                if (_currentState != CardSelectionState.SwapTheCards)
                {
                    if (!IsAnyCardUnderMouse())
                    {
                        // Clicked on empty space, which will deselect and return to Idle
                        _isInActionMenu = true;
                        KeyboardModeCancelled?.Invoke(true);
                        DeactivateKeyboardMode();
                        ChangeState(CardSelectionState.Idle);
                    }
                }

                break;
            }
        }
    }

    public void SetCardHand(Array<Node> cardHand)
    {
        foreach (Node node in cardHand)
        {
            if (node is Card card) _cardHand.Add(card);
        }
    }

    public void SetCenterCard(Card centerCard)
    {
        (_keyboardHoveredCard, _centerCard) = (centerCard, centerCard);
        CurrentSelectedCardChanged?.Invoke(centerCard);
    } 
    
    // Used in BattleScene.cs, Selects the Center Card when Action Menu shows up
    public void SelectCenterCard()
    {
        _isInActionMenu = false;
        SelectCard(_centerCard);
    }
    
    // Used in BattleScene.cs, Deselects the Center Card when Action Menu leaves
    public void DeselectCenterCard()
    {
        ApplyVisualState(_centerCard, false);
    }

    private void DeselectCard()
    {
        if (_selectedCard == null) return;

        Card oldSelected = _selectedCard;

        ApplyVisualState(oldSelected, false);

        _selectedCard = null;
        
        KeyboardHoveredCardChanged?.Invoke(_centerCard);
        SelectionChanged?.Invoke(oldSelected, _selectedCard);
    }
    
    private void ChangeState(CardSelectionState newState)
    {
        _currentState = newState;

        if (_currentState == CardSelectionState.SwapTheCards)
        {
            SwappingStateChanged?.Invoke(true);
        }
        
        else if (_currentState != CardSelectionState.SwapTheCards)
        {
            SwappingStateChanged?.Invoke(false);
        }
    }
}