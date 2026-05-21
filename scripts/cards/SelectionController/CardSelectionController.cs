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

public sealed partial class CardSelectionController : Node2D
{
    private const float SelectionTweenDuration = 0.15f;
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
    private Dictionary<Card, Tween> _activePositionTweens = new();
    
    public Array<Card> CardHand { get; private set; }
    public Card CenterCard { get; private set; }

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
    
    public void SetCenterCard(Card centerCard) => (_keyboardHoveredCard, CenterCard) = (centerCard, centerCard);
    
    // Used in BattleScene.cs, Selects the Center Card when Action Menu shows up
    public void SelectCenterCard()
    {
        SelectCard(CenterCard);
        
        ChangeState(CardSelectionState.Idle);
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
        
        EmitSignal(SignalName.GetKeyboardHoveredCard, CenterCard);

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


