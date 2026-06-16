using Godot;
using GodotUtilities;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Cards;

/// <summary>
/// Manages hover effects (scale tweening + ZIndex changes) for the cards.
/// Only the topmost unselected card under the mouse gets the effect.
/// </summary>
[Scene]
public partial class CardHoverController : Node
{
    // State machines
    private enum CardHoverPhase { Inactive, ActionMenu, CardSelection }
    private enum HoverMode { None, Mouse, Keyboard }

    private CardHoverPhase _currentCardHoverPhase = CardHoverPhase.Inactive;
    private HoverMode _currentHoverMode = HoverMode.None;
    
    [Node("CardBorder")] private Sprite2D _cardBorder;

    private readonly Dictionary<Card, Tween> _activeHoverTweens = new();
    private const float HoverTweenDuration = 0.4f;

    private static readonly Vector2 CardHoveredScale = new(1.1f, 1.1f);
    private const float MaxCardScale = 0.45f;

    private static readonly Vector2 CardBorderSubtract = new(.7f, .7f);
    private const float CardBorderTweenDuration = 0.15f;
    private const float CardBorderFadeDuration = 0.10f;

    private Vector2 _cardBorderOriginalScale;

    private CardSystem _cardSystem;

    private readonly HashSet<Card> _cardsUnderMouse = new();
    private Card _keyboardHoveredCard;
    private Card _mouseHoveredCard;
    private Card _selectedCard;

    public Godot.Collections.Dictionary<Card, int> OriginalZIndexes = new();

    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated) WireNodes();
    }

    public override void _Ready()
    {
        _cardSystem = GetNode<CardSystem>("..");
        _cardBorder.Visible = true;
        _cardBorderOriginalScale = _cardBorder.Scale;
    }

    public void SetCenterCard(Card centerCard) => _cardBorder.GlobalPosition = centerCard.GlobalPosition;

    #region State Transitions

    public void OnSwappingStateChanged(bool isSwapping)
    {
        ChangeCardHoverPhase(isSwapping ? CardHoverPhase.Inactive : CardHoverPhase.CardSelection);
    }

    public void OnCardSelected(Card card)
    {
        _cardsUnderMouse.Remove(card);
        KillAndRemoveTween(card);
        
        _selectedCard = card;
        ChangeCardHoverPhase(CardHoverPhase.CardSelection);
        UpdateHoverEffect();
        
        if (_currentCardHoverPhase == CardHoverPhase.CardSelection)
             EnteredCardSelection();
    }

    public void OnCardDeselected()
    {
        _selectedCard = null;
        ChangeCardHoverPhase(CardHoverPhase.Inactive);
    }

    public void OnActionMenuOpened()
    {
        ExitCardSelection();
        ChangeCardHoverPhase(CardHoverPhase.ActionMenu);
    }
    
    public void OnActionMenuClosed()
    {
        ChangeCardHoverPhase(CardHoverPhase.CardSelection);
        EnteredCardSelection();
    }

    private void ChangeCardHoverPhase(CardHoverPhase newPhase)
    {
        if (_currentCardHoverPhase == newPhase) return;
        
        GD.Print($"[CardHoverController] Phase changed: {_currentCardHoverPhase} → {newPhase}");
        
        // Exit old phase
        if (_currentCardHoverPhase ==  CardHoverPhase.CardSelection)
        {
            ChangeHoverMode(HoverMode.None);
        }

        _currentCardHoverPhase = newPhase;
    }

    private void ChangeHoverMode(HoverMode newMode)
    {
        if (_currentHoverMode == newMode) return;
        
        // Clean up previous mode
        switch (_currentHoverMode)
        {
            case HoverMode.Mouse:
                if (_mouseHoveredCard != null)
                {
                    ApplyHoverEffect(_mouseHoveredCard, false, false);
                }
                
                _mouseHoveredCard = null;
                break;
            case HoverMode.Keyboard:
                ClearKeyboardHover();
                break;
        }

        _currentHoverMode = newMode;
    }

    #endregion

    #region Hover-Related Functions

    #region Mouse Mode
    
    public void OnHoveredOverCard(Card card)
    {
        if (_currentCardHoverPhase != CardHoverPhase.CardSelection) return;
        if (_currentHoverMode == HoverMode.Keyboard) return;

        ChangeHoverMode(HoverMode.Mouse);
        _cardsUnderMouse.Add(card);
        UpdateHoverEffect();
    }

    public void OnHoveredOffCard(Card card)
    {
        if (_currentCardHoverPhase != CardHoverPhase.CardSelection) return;
        if (_currentHoverMode == HoverMode.Keyboard) return;

        _cardsUnderMouse.Remove(card);
        UpdateHoverEffect();
    }
    
    private void UpdateHoverEffect()
    {
        if (_currentCardHoverPhase != CardHoverPhase.CardSelection) return;
        if (_currentHoverMode == HoverMode.Keyboard) return;

        if (_cardsUnderMouse.Count == 0)
        {
            if (_mouseHoveredCard != null)
            {
                ApplyHoverEffect(_mouseHoveredCard, false, false);
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
            if (_mouseHoveredCard != null) ApplyHoverEffect(_mouseHoveredCard, false, false);

            if (highestCard != null) ApplyHoverEffect(highestCard, true, false);

            _mouseHoveredCard = highestCard;
        }
    }
    
    #endregion

    #region Keyboard Mode
    
    public void KeyboardHover(Card card)
    {
        if (_currentCardHoverPhase != CardHoverPhase.CardSelection) return;
        ChangeHoverMode(HoverMode.Keyboard);

        ClearKeyboardHover(); // clears previous the card's hover effect

        if (card == _selectedCard)
        {
            ApplyBorderEffect(card, card.Scale);
            return;
        }

        _keyboardHoveredCard = card;
        ApplyHoverEffect(_keyboardHoveredCard, true, true);
    }

    public void ClearKeyboardHover()
    {
        if (_keyboardHoveredCard == null) return;

        Card highlightedCard = _keyboardHoveredCard;
        _keyboardHoveredCard = null;

        UpdateHoverEffect();
        ApplyHoverEffect(highlightedCard, false, false);
    }
    
    #endregion

    // VISUALS
    private void ApplyHoverEffect(Card card, bool isHovered, bool isFromKeyboardMode)
    {
        KillAndRemoveTween(card);
        Vector2 cardScale = isHovered ? CardHoveredScale : Vector2.One;

        Tween tween = CreateTween();

        tween.TweenProperty(card, "scale", cardScale, HoverTweenDuration)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        if (isFromKeyboardMode) ApplyBorderEffect(card, CardHoveredScale);

        AddTween(card, tween);

        card.ZIndex = isHovered ? CardSystem.HoverZ : OriginalZIndexes[card];
        card.UpdatePriority();
    }

    #endregion

    #region CardBorder-Related Functions

    public async void EnteredCardSelection()
    {
        await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
        
        Tween tween = CreateTween().SetParallel();
        tween.TweenProperty(_cardBorder, "modulate:a", 1.0f, CardBorderFadeDuration);

        ApplyBorderEffect(_selectedCard, _selectedCard.Scale);
    }

    public void ExitCardSelection()
    {
        Tween tween = CreateTween().SetParallel();
        tween.TweenProperty(_cardBorder, "modulate:a", 0.0f, CardBorderFadeDuration);
    }

    private void ApplyBorderEffect(Card card, Vector2 cardScale)
    {
        Vector2 newCardBorderScale = cardScale - CardBorderSubtract;

        float cardBorderClampedX = Mathf.Clamp(newCardBorderScale.X, _cardBorderOriginalScale.X, MaxCardScale);
        float cardBorderClampedY = Mathf.Clamp(newCardBorderScale.Y, _cardBorderOriginalScale.Y, MaxCardScale);

        Tween tween = CreateTween().SetParallel();

        tween.TweenProperty(_cardBorder, "global_rotation", card.GlobalRotation, CardBorderTweenDuration);

        tween.TweenProperty(_cardBorder, "global_position", card.GlobalPosition, CardBorderTweenDuration)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(_cardBorder, "scale", new Vector2(cardBorderClampedX, cardBorderClampedY), CardBorderTweenDuration);
    }

    #endregion

    #region Tween Helpers

    private void KillAndRemoveTween(Card card)
    {
        if (_activeHoverTweens.ContainsKey(card) && _activeHoverTweens[card].IsRunning())
        {
            _activeHoverTweens[card].Kill();
        }
        
        _activeHoverTweens.Remove(card);
    }

    private void AddTween(Card card, Tween tween)
    {
        KillAndRemoveTween(card);

        _activeHoverTweens[card] = tween;
        tween.Finished += () =>
        {
            if (_activeHoverTweens.TryGetValue(card, out Tween activeTween)
                && activeTween == tween)
            {
                _activeHoverTweens.Remove(card);
            }
        };
    }
    #endregion
}