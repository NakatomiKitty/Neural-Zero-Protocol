using Godot;
using GodotUtilities;
using Godot.Collections;
using System.Collections.Generic;

// TODO: MAKE THIS MORE READABLE, HAVE SOMEONE CODE REVIEW IT
namespace NeuralZeroProtocol.Scripts.Cards;

/// <summary>
/// Manages hover effects (scale tweening + ZIndex changes) for the cards.
/// Only the topmost unselected card under the mouse gets the effect.
/// </summary>
[Scene]
public partial class CardHoverController : Node
{
	[Node("CardBorder")] private Sprite2D _cardBorder;
	
    private readonly System.Collections.Generic.Dictionary<Card, Tween> _activeHoverTweens = new();
    private const float HoverTweenDuration = 0.4f;
    
    private static readonly Vector2 CardHoveredScale = new(1.1f, 1.1f);
    private const float MaxCardScale = 0.45f;
    
    private static readonly Vector2 CardBorderSubtract = new(.7f, .7f);
    private const float CardBorderTweenDuration = 0.15f;
    private const float CardBorderFadeDuration = 0.10f;
    
    private Vector2 _cardBorderSelectedPosition; // Border will go here instead of selected card's position
    private Vector2 _cardBorderOriginalScale;
    
	private CardSystem _cardSystem;
	
	private readonly HashSet<Card> _cardsUnderMouse = new();
    private Card _keyboardHoveredCard; // Used for selecting with keyboard
    private Card _mouseHoveredCard; // Used for selecting with mouse 
	private Card _selectedCard;

	private bool _onInitialSelected;
	private bool _hasLeftMenuMode = true;
	private bool _isSwapping;
    
    
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
	
    public void OnSwappingStateChanged(bool isSwapping) => _isSwapping = isSwapping;

    #region Hover-Related Functions
    
    #region Mouse Mode Functions
	public void OnCardSelected(Card card)
	{
		// Selected card no longer gets the hover effect 
        _cardsUnderMouse.Remove(card);

        if (_mouseHoveredCard == card)
		{
			if (_mouseHoveredCard != null)
			{
				ApplyHoverEffect(_mouseHoveredCard, false, false);
			}
			_mouseHoveredCard = null;
		}
        
        KillAndRemoveTween(card);

		_selectedCard = card;
		
		if (_hasLeftMenuMode)
		{
			EnteredCardSelection();
		}
		
		UpdateHoverEffect();
	}

	public void OnCardDeselected()
	{
		_selectedCard = null;
		UpdateHoverEffect();
	}
	
	public void OnHoveredOverCard(Card card)
	{
		if (_isSwapping) return;
		if (card == _keyboardHoveredCard) return;
		if (card == _selectedCard) return;   // Never hover the selected card

		_cardsUnderMouse.Add(card);
		UpdateHoverEffect();
	}

	public void OnHoveredOffCard(Card card)
	{
		if (_isSwapping) return;
		if (card == _keyboardHoveredCard) return;
		if (card == _selectedCard) return; 

		_cardsUnderMouse.Remove(card);
		UpdateHoverEffect();
	}
	
	private void UpdateHoverEffect()
	{
		if (_isSwapping) return;
		if (_keyboardHoveredCard != null) return;
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
			if(_mouseHoveredCard != null) ApplyHoverEffect(_mouseHoveredCard, false, false);
            
			if (highestCard != null) ApplyHoverEffect(highestCard, true, false);

			_mouseHoveredCard = highestCard;
		}
	}
	
	#endregion
	
	#region Keyboard Mode Functions
	
    public void KeyboardHover(Card card)
    {
        ClearKeyboardHover();
        
        if (card == _keyboardHoveredCard) return;
        if (card == _selectedCard)
        {
			ApplyBorderEffectOnSelectedCard();
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
	
    // Visual Function
    private void ApplyHoverEffect(Card card, bool isHovered, bool isFromKeyboardMode)
    {
        KillAndRemoveTween(card);
        Vector2 cardScale = isHovered ? CardHoveredScale : Vector2.One;
        
        Tween tween = CreateTween();
        
        tween.TweenProperty(card, "scale", cardScale, HoverTweenDuration)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
        
        if (isFromKeyboardMode)
        {
	        ApplyBorderEffect(card, CardHoveredScale);
        }
        
        AddTween(card, tween);

        card.ZIndex = isHovered ? CardSystem.HoverZ : OriginalZIndexes[card];

        card.UpdatePriority();
    }
    
    #endregion

    #region CardBorder-Related Functions
    
    // Makes the Border appear when you enter Card Selection
    public async void EnteredCardSelection(bool fromMenu = false)
    {
	    await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
	    
	    Tween tween = CreateTween().SetParallel();
	    tween.TweenProperty(_cardBorder, "modulate:a", 1.0f, CardBorderFadeDuration);
	    
	    if (fromMenu) return;
	    
	    ApplyBorderEffect(_selectedCard, _selectedCard.Scale);
	    _hasLeftMenuMode = false;
    }

    public void ExitCardSelection()
    {
	    _hasLeftMenuMode = true;
	    
	    Tween tween = CreateTween().SetParallel();
	    tween.TweenProperty(_cardBorder, "modulate:a", 0.0f, CardBorderFadeDuration);
		
	    tween.Finished += ApplyBorderEffectOnSelectedCard;
    }

    private void ApplyBorderEffectOnSelectedCard()
    {
	    Tween tween = CreateTween().SetParallel();
	    
	    tween.TweenProperty(_cardBorder, "global_rotation", 0.0f, CardBorderFadeDuration);
	    
	    tween.TweenProperty(_cardBorder, "global_position", _cardBorderSelectedPosition, CardBorderTweenDuration)
		    .SetTrans(Tween.TransitionType.Quint)
		    .SetEase(Tween.EaseType.Out);
	    
	    tween.TweenProperty(_cardBorder, "scale", new Vector2(MaxCardScale, MaxCardScale), CardBorderTweenDuration);

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

	    tween.Finished += () =>
	    {
			if (_onInitialSelected) return;
			
			_cardBorderSelectedPosition = card.GlobalPosition;
			
			GD.Print(_cardBorderSelectedPosition);
			_onInitialSelected = true; // only once, on start up
	    };
    }
	
	#endregion
	
	private void KillAndRemoveTween(Card card)
	{
		if (_activeHoverTweens.ContainsKey(card) && _activeHoverTweens[card].IsRunning())
			_activeHoverTweens[card].Kill();

		_activeHoverTweens.Remove(card);
	}
	
	private void AddTween(Card card, Tween tween)
	{
		KillAndRemoveTween(card);

		_activeHoverTweens[card] = tween;
		tween.Finished += () => _activeHoverTweens.Remove(card);
	}
}


