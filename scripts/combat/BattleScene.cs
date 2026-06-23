using Godot;
using Godot.Collections;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Cards;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using NeuralZeroProtocol.Scripts.Ui;

namespace NeuralZeroProtocol.Scripts.Combat;

[Scene]
public partial class BattleScene : Node2D
{
    private const float MenuAndCardSystemMoveTweenDuration = 0.5f;
    private const float ClonedCardMoveTweenDuration = 0.6f;
    
    [Node] public TurnManager TurnManager;
    [Node] public CombatStateMachine CombatStateMachine;
    [Node] public BattleManager BattleManager;
    [Node] public CardSystem CardSystem;
    [Node] public UiSelectionController UiSelectionController;
    [Node("CanvasLayer/BattleMenu")] public BattleMenu BattleMenu;
    
    private MoveResource[] _moves;
    private Tween _cardSystemTween;
    
    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated) WireNodes();
    }
    
    public override void _Ready() 
    {
        foreach (Character character in TurnManager.AllUnits)
		{
            // Links every Character's HealthComponent's Died signal in the battle
			character.HealthComponent.Died += CombatStateMachine.OnCharacterDied;
		}
        
        CardSystem.Visible = false;
        TurnManager.StartBattle += OnBattleStart;
        BattleMenu.RequestingCardBorderRemoval += CardSystem.OnKeyboardModeDeactivated;
        BattleMenu.ActionMenuStateChanged += OnActionMenuStateChangedForHover;
        BattleMenu.ActionMenuStateChanged += OnActionMenuStateChanged;
        BattleMenu.MovingToCardSystem += CardSystem.OnMoveToCardSystem;
        
        CardSystem.CardSelectionController.CurrentSelectedCardChanged += CombatStateMachine.CharacterAttackHandler.SetCurrentSelectedCard;
        CardSystem.CardSelectionController.KeyboardModeCancelled += BattleMenu.OnKeyboardModeCancelled;
        CardSystem.CardSelectionController.KeyboardModeActivated += BattleMenu.OnCardKeyboardModeActivated;
        CardSystem.CardSelectionController.KeyboardModeDeactivated += BattleMenu.OnCardKeyboardModeDeactivated;

        CombatStateMachine.Initialize(this);
        CombatStateMachine.AttackTriggered += OnAttackTriggeredPlayMenuSequence;
        
        // Passes the array to CardSystem so it can be used to update the card skins
        Array<MoveResource> moves = BattleManager.GetSelectedMovesFromCurrentChar(TurnManager.PlayerCharacters);
        _ = CardSystem.CreateHandFromMoves(moves);
        
        TurnManager.StartBattleSequence();
    }
    
    public override void _Input(InputEvent @event) 
    {
        CardSystem.CardSelectionController.GetUiInput(UiSelectionController.GetUiSelect());
        BattleMenu.GetUiInput(UiSelectionController.GetUiSelect());
    }

    private void OnActionMenuStateChangedForHover(bool isOpen)
    {
        if (isOpen)
        {
            CardSystem.CardHoverController.OnActionMenuOpened();
        }
        else
        {
            CardSystem.CardHoverController.OnActionMenuClosed();
        }
    }
    
    private void OnBattleStart()
    {
        Character firstUnit = TurnManager.GetCurrentUnit();
        CombatStateMachine.SetCurrentCharacter(firstUnit);
        CombatStateMachine.StartBattle();
    }

    private void OnActionMenuStateChanged(bool isTrue)
    {
        float offset = isTrue ? -32 : 32;
        if (!isTrue) CardSystem.CardSelectionController.DeselectCenterCard();
        MoveCardSystemRelativeToButtonContainer(offset, isTrue);
    }
    
    private void MoveCardSystemRelativeToButtonContainer(float offset,  bool isTrue)
    {
        float buttonGlobalY = isTrue ? 
            BattleMenu.ActionMenuContainer.GlobalPosition.Y : 
            BattleMenu.BattleCommandMenuContainer.GlobalPosition.Y;
        
        // Desired global position for CardSystem
        float targetGlobalY = buttonGlobalY + offset;
    
        // Convert to CardSystem's parent local coordinates
        Node2D parent = CardSystem.GetParent<Node2D>();
        float targetLocalY = parent.ToLocal(new Vector2(0, targetGlobalY)).Y;

        Vector2 newCardSystemPosition = new(CardSystem.Position.X, targetLocalY);
        CardSystemTween(newCardSystemPosition, isTrue);
    }

    private void CardSystemTween(Vector2 newCardSystemPosition, bool isTrue)
    {
        _cardSystemTween?.Kill();
        _cardSystemTween = CreateTween();
        CardSystem.Visible = true;
        
        _cardSystemTween.TweenProperty(CardSystem, "position", newCardSystemPosition, MenuAndCardSystemMoveTweenDuration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);

        _cardSystemTween.Finished += () =>
        {
            CardSystem.Visible = isTrue;

            if (isTrue) CardSystem.CardSelectionController.SelectCenterCard();
        };
    }

    private void OnAttackTriggeredPlayMenuSequence()
    {
        // Removes the border
        CardSystem.CardBorderController.OnExitCardSelection();
        OnActionMenuStateChanged(false);
        BattleMenu.ChangeState(BattleMenu.MenuState.PlayerTurnEnd);
        
        Card clonedCenterCard = CardSystem.DuplicateCenterCard();
        AddChild(clonedCenterCard);
        
        CardUsedUpAnimation(clonedCenterCard);
    }

    private async void CardUsedUpAnimation(Card clonedCenterCard)
    {
        Rect2 visibleRect = GetViewport().GetVisibleRect();
        
        Sprite2D sprite = clonedCenterCard.GetNode<Sprite2D>("CardImage");
        float cardHeight = sprite.GetRect().Size.Y;
        
        // This makes the card's bottom edge exactly at the top of the screen (Hence, off screen)
        float targetGlobalY = visibleRect.Position.Y - cardHeight;
        
        Tween tween1 = CreateTween();
        
        tween1.TweenProperty(clonedCenterCard, "global_position:y", targetGlobalY, ClonedCardMoveTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.In);
        
        await ToSignal(tween1, Tween.SignalName.Finished);
        
        float thirdWidth = visibleRect.Size.X / 3f;
        float leftEdgeOfThird = visibleRect.Position.X + (2f * thirdWidth); // left edge of the 3rd third
        float targetGlobalX = leftEdgeOfThird + (thirdWidth / 2f);          
            
        clonedCenterCard.Scale -= Vector2.One * 0.075f;
        clonedCenterCard.GlobalPosition = new Vector2(targetGlobalX, -cardHeight);
            
        float targetY = visibleRect.Position.Y + (visibleRect.Size.Y / 2f);
        Tween tween2 = CreateTween();
        tween2.TweenProperty(clonedCenterCard, "global_position:y", targetY, ClonedCardMoveTweenDuration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);
    }
    public override void _ExitTree()
    {
        TurnManager.StartBattle -= OnBattleStart;
        BattleMenu.ActionMenuStateChanged -= OnActionMenuStateChangedForHover;
        BattleMenu.ActionMenuStateChanged -= OnActionMenuStateChanged;
        BattleMenu.MovingToCardSystem -= CardSystem.OnMoveToCardSystem;
        
        CardSystem.CardSelectionController.CurrentSelectedCardChanged -= CombatStateMachine.CharacterAttackHandler.SetCurrentSelectedCard;
        CardSystem.CardSelectionController.KeyboardModeCancelled -= BattleMenu.OnKeyboardModeCancelled;
        CardSystem.CardSelectionController.KeyboardModeActivated -= BattleMenu.OnCardKeyboardModeActivated;
        CardSystem.CardSelectionController.KeyboardModeDeactivated -= BattleMenu.OnCardKeyboardModeDeactivated;
    }
}


