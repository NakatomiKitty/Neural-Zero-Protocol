using Godot;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Cards;
using NeuralZeroProtocol.Scripts.Characters;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using NeuralZeroProtocol.Scripts.Ui;

namespace NeuralZeroProtocol.Scripts.Combat;

[Scene]
public partial class BattleScene : Node2D
{
    private const float MoveTweenDuration = 0.5f;
    
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
        BattleMenu.ActionMenuState += OnActionMenuState;
        BattleMenu.MoveToCardSystem += CardSystem.OnMoveToCardSystem;
        
        CardSystem.CardSelectionController.CurrentSelectedCardChanged += CombatStateMachine.CharacterAttackHandler.SetCurrentSelectedCard;
        CardSystem.CardSelectionController.KeyboardModeCancelled += BattleMenu.OnKeyboardModeCancelled;
        CardSystem.CardSelectionController.KeyboardModeActivated += BattleMenu.OnCardKeyboardModeActivated;
        CardSystem.CardSelectionController.KeyboardModeDeactivated += BattleMenu.OnCardKeyboardModeDeactivated;

        CombatStateMachine.AttackTriggered += OnAttackTriggeredPlayMenuSequence;
        
        // Passes the array to CardSystem so it can be used to update the card skins
        _ = CardSystem.CreateHandFromMoves(BattleManager.GetSelectedMovesFromCurrentChar());
        
        TurnManager.StartBattleSequence();
    }
    
    public override void _Input(InputEvent @event) 
    {
        CardSystem.CardSelectionController.GetUiInput(UiSelectionController.GetUiSelect());
        BattleMenu.GetUiInput(UiSelectionController.GetUiSelect());
    }

    private void OnBattleStart()
    {
        Character firstUnit = TurnManager.GetCurrentUnit();
        CombatStateMachine.SetCurrentCharacter(firstUnit);
        CombatStateMachine.StartBattle();
    }

    private void OnActionMenuState(bool isTrue)
    {
        float offset = isTrue ? -32 : 32;
        if (!isTrue) CardSystem.CardSelectionController.DeselectCenterCard();
        MoveCardSystemRelativeToButtonContainer(offset, isTrue);
    }
    
    private void MoveCardSystemRelativeToButtonContainer(float offset,  bool isTrue)
    {
        float buttonGlobalY = isTrue ? 
            BattleMenu.ActionMenuContainer.GlobalPosition.Y : 
            BattleMenu.InitialMenuContainer.GlobalPosition.Y;
        
        // Desired global position for CardSystem
        float targetGlobalY = buttonGlobalY + offset;
    
        // Convert to CardSystem's parent local coordinates
        Node2D parent = CardSystem.GetParent<Node2D>();
        float targetLocalY = parent.ToLocal(new Vector2(0, targetGlobalY)).Y;
    
        // Apply the position
        Vector2 newCardSystemPosition = new(CardSystem.Position.X, targetLocalY);
        CardSystemTween(newCardSystemPosition, isTrue);
    }

    private void CardSystemTween(Vector2 newCardSystemPosition, bool isTrue)
    {
        _cardSystemTween?.Kill();
        _cardSystemTween = CreateTween();
        CardSystem.Visible = true;
        
        _cardSystemTween.TweenProperty(CardSystem, "position", newCardSystemPosition, MoveTweenDuration)
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
        CardSystem.CardHoverController.ExitCardSelection();
        MoveCardSystemRelativeToButtonContainer(-32, false);
        BattleMenu.ForceMenuToOriginalPositions();
        
        Card clonedCenterCard = CardSystem.DuplicateCenterCard();
        AddChild(clonedCenterCard);
        
    }
    
    public override void _ExitTree()
    {
        TurnManager.StartBattle -= OnBattleStart;
        BattleMenu.ActionMenuState -= OnActionMenuState;
        BattleMenu.MoveToCardSystem -= CardSystem.OnMoveToCardSystem;
        
        CardSystem.CardSelectionController.CurrentSelectedCardChanged -= CombatStateMachine.CharacterAttackHandler.SetCurrentSelectedCard;
        CardSystem.CardSelectionController.KeyboardModeCancelled -= BattleMenu.OnKeyboardModeCancelled;
        CardSystem.CardSelectionController.KeyboardModeActivated -= BattleMenu.OnCardKeyboardModeActivated;
        CardSystem.CardSelectionController.KeyboardModeDeactivated -= BattleMenu.OnCardKeyboardModeDeactivated;
    }
}


