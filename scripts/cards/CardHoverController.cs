using System;
using Godot;
using System.Collections.Generic;

namespace NeuralZeroProtocol.Scripts.Cards;

public partial class CardHoverController : Node
{
    // State machines
    private enum CardHoverPhase { Inactive, ActionMenu, CardSelection }
    private enum HoverMode { None, Mouse, Keyboard }

    private CardHoverPhase _currentCardHoverPhase = CardHoverPhase.Inactive;
    private HoverMode _currentHoverMode = HoverMode.None;

    private readonly Dictionary<Card, Tween> _activeHoverTweens = new();
    private const float HoverTweenDuration = 0.4f;
    private static readonly Vector2 CardHoveredScale = new(1.1f, 1.1f);

    private CardSystem _cardSystem;

    public event Action<Card> EnterCardSelection; // Card selectedCard
    public event Action ExitCardSelection;
    public event Action<Card, Vector2> ApplyBorderEffect; // Card card, Vector2 cardScale
    
    private readonly HashSet<Card> _cardsUnderMouse = new();
    private Card _keyboardHoveredCard;
    private Card _mouseHoveredCard;
    private Card _selectedCard;

    public Godot.Collections.Dictionary<Card, int> OriginalZIndexes = new();

    public void Initialize(CardSystem cardSystem) => _cardSystem = cardSystem;

    #region State Transitions

    public void OnSwappingStateChanged(bool isSwapping)
    {
        ChangeCardHoverPhase(isSwapping ? CardHoverPhase.Inactive : CardHoverPhase.CardSelection);
    }

    public void OnCardSelected(Card card)
    {
        _cardsUnderMouse.Remove(card);

        if (_mouseHoveredCard == card)
        {
            ApplyHoverEffect(card, false, false);
            _mouseHoveredCard = null;
        }
        
        KillAndRemoveTween(card);
        
        _selectedCard = card;
        ChangeCardHoverPhase(CardHoverPhase.CardSelection);
        UpdateHoverEffect();
    }

    public void OnCardDeselected()
    {
        _selectedCard = null;
        ChangeCardHoverPhase(CardHoverPhase.Inactive);
    }

    public void OnActionMenuOpened()
    {
        ExitCardSelection?.Invoke();
        ChangeCardHoverPhase(CardHoverPhase.ActionMenu);
    }
    
    public void OnActionMenuClosed()
    {
        ChangeCardHoverPhase(CardHoverPhase.CardSelection);
        EnterCardSelection?.Invoke(_selectedCard);
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
        
        GD.Print($"[CardHoverController] HoverMode changed: {_currentHoverMode} → {newMode}");
        
        // Exit old mode
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
            ApplyBorderEffect?.Invoke(card, card.Scale);
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

        if (isFromKeyboardMode) ApplyBorderEffect?.Invoke(card, CardHoveredScale);

        AddTween(card, tween);

        card.ZIndex = isHovered ? CardSystem.HoverZ : OriginalZIndexes[card];
        card.UpdatePriority();
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