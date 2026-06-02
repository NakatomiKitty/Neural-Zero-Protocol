using System.Linq;
using Godot;
using Godot.Collections;

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

public sealed partial class CardSelectionController : Node2D
{
    private const float MouseMovementThreshold = 5.0f;
    
    private const float SelectionTweenDuration = 0.15f;
    private const float SwapTweenDuration = 0.15f;
    
    private const float SelectedCardSizeMultiplier = 1.15f;
    private const int SelectedCardVerticalOffset = -20;
    
    [Signal] public delegate void SelectionChangedEventHandler(Card oldSelectedCard, Card newSelectedCard);
    
    //
    [Signal] public delegate void KeyboardHoveredCardChangedEventHandler(Card card);
    [Signal] public delegate void CurrentSelectedCardChangedEventHandler(Card currentSelectedCard);
    [Signal] public delegate void SwappingStateChangedEventHandler(bool isSwapping);
    [Signal] public delegate void KeyboardModeDeactivatedEventHandler();
    [Signal] public delegate void KeyboardModeCancelledEventHandler(bool isCancelled);
    [Signal] public delegate void KeyboardModeActivatedEventHandler();
    
    private Dictionary<Card, Tween> _activePositionTweens = new();
    private CardSelectionState _state;
    
    private Tween _swapCardTween;
        
    private Card _selectedCard; // Used for selecting with mouse
    private Card _keyboardHoveredCard; // Used for selecting with keyboard
    
    private bool _isInActionMenu;
    
    private Vector2 _activationMousePos;
    private bool _ignoreMouseMotion;

    
    
    public Dictionary<Card, Vector2> CardBasePositions = new();
    public Dictionary<Card, int> OriginalZIndexes = new();
    
    public Array<Card> CardHand = new();
    public Card CenterCard;

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
            case InputEventMouseMotion motion when _state == CardSelectionState.KeyboardMode:
                if (!_ignoreMouseMotion && _activationMousePos.DistanceTo(motion.GlobalPosition) < MouseMovementThreshold)
                {
                    return;   
                }
                _ignoreMouseMotion = true;
                
                _isInActionMenu = true;
                EmitSignal(SignalName.KeyboardModeCancelled, true);
                DeactivateKeyboardMode();
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }:
            {
                // If we're in a state that allows deselection (e.g., not swapping)
                if (_state != CardSelectionState.SwapTheCards)
                {
                    if (!IsAnyCardUnderMouse())
                    {
                        // Clicked on empty space, which will deselect and return to Idle
                        _isInActionMenu = true;
                        EmitSignal(SignalName.KeyboardModeCancelled, true);
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
            if (node is Card card) CardHand.Add(card);
        }
    }

    public void SetCenterCard(Card centerCard)
    {
        (_keyboardHoveredCard, CenterCard) = (centerCard, centerCard);
        EmitSignal(SignalName.CurrentSelectedCardChanged, centerCard);
    } 
    
    // Used in BattleScene.cs, Selects the Center Card when Action Menu shows up
    public void SelectCenterCard()
    {
        _isInActionMenu = false;
        SelectCard(CenterCard);
    }
    
    // Used in BattleScene.cs, Deselects the Center Card when Action Menu leaves
    public void DeselectCenterCard()
    {
        ApplyVisualState(CenterCard, false);
    }
    
    public void DeselectCard()
    {
        if (_selectedCard == null) return;

        Card oldSelected = _selectedCard;

        ApplyVisualState(oldSelected, false);

        _selectedCard = null;
        
        EmitSignal(SignalName.KeyboardHoveredCardChanged, CenterCard);

        EmitSignal(SignalName.SelectionChanged, oldSelected, _selectedCard);
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
}