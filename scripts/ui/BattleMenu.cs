using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace NeuralZeroProtocol.Scripts.Ui;

public enum ActionType { Switch, Moves, Run, Block, Attack, Evade }
public enum MenuState { PlayerTurnStart, BattleCommandMenu, Swapping, ActionMenu, PlayerTurnEnd }

public partial class BattleMenu : Control
{
    [Signal] public delegate void ActionSelectedEventHandler(int actionType);
    public event Action<bool> ActionMenuState;
    public event Action MoveToCardSystem;
    
    public Control ActionMenuContainer;
    public Control BattleCommandMenuContainer;
    private Vector2 _originalMenuPosition;
    private float _targetMenuPositionY;
    
    private const float MenuSwapTween = 0.5f;
    private const float MenuForceExitTween = 2f;
    
    private const string InitialFocusButton = "MovesButton";
    private const string ActionMenuDefaultFocus = "AttackButton";
    
    private MenuState _currentMenuState;
    private MenuState _targetMenuState;
    
    private List<TextureButton> _actionButtons;
    
    private bool _isBattleCommandMenuKeyboardMode;    // BattleCommandMenu keyboard mode
    private bool _isActionMenuKeyboardMode;     // ActionMenu keyboard mode
    private bool _isCardKeyboardMode;
    
    private bool _neighborsSetup;
    
    public override void _Ready()
    {
        BattleCommandMenuContainer = GetNode<Control>("InitialButtonContainer");
        ActionMenuContainer = GetNode<Control>("SecondaryButtonContainer");
        _originalMenuPosition = ActionMenuContainer.GlobalPosition;
        
        GetContainerChildren(BattleCommandMenuContainer);
        GetContainerChildren(ActionMenuContainer);
        GetTargetMenuPosition();
        SetUpActionButtons();
        
        ChangeState(MenuState.PlayerTurnStart);
    }
    
    #region Outside Signals
    public void OnCardKeyboardModeActivated()
    {
        if (_currentMenuState == MenuState.ActionMenu)
        {
            _isCardKeyboardMode = true;
            _isActionMenuKeyboardMode = false;
            
            SetMenuButtonsEnabled(ActionMenuContainer, false);
        }
    }
    
    public async void OnCardKeyboardModeDeactivated()
    {
        if (_currentMenuState == MenuState.ActionMenu)
        {
            await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
            
            _isCardKeyboardMode = false;
            
            FocusOnAttackButton();
        }
    }
    
    public void OnKeyboardModeCancelled(bool isCancelledByMouse)
    {
        if (!isCancelledByMouse) return;
        
        if (_currentMenuState == MenuState.BattleCommandMenu)
        {
            _isBattleCommandMenuKeyboardMode = false;
            
            ReleaseFocusFromContainer(BattleCommandMenuContainer);
        }
        else if (_currentMenuState == MenuState.ActionMenu)
        {
            _isCardKeyboardMode = false;
            _isActionMenuKeyboardMode = false;
            
            ReleaseFocusFromContainer(ActionMenuContainer);
        }
    }
    #endregion

    #region Public Methods
    public void GetUiInput(UiSelection uiSelection)
    {
        if (uiSelection == UiSelection.None) return;
        
        switch (_currentMenuState)
        {
            case MenuState.BattleCommandMenu:
                HandleBattleCommandMenuInput();
                break;
            case MenuState.ActionMenu:
                HandleActionMenuInput(uiSelection);
                break;
        }
    }

    public void ForceMenuToOriginalPositions()
    {
        Tween tween = CreateTween().SetParallel();
        
        MenuLerp(BattleCommandMenuContainer, _originalMenuPosition, MenuForceExitTween, tween);
        MenuLerp(ActionMenuContainer, _originalMenuPosition, MenuForceExitTween, tween);
    }
    #endregion

    #region State Machine and States

    public void ChangeState(MenuState newState)
    {
        _currentMenuState = newState;
        
        switch (_currentMenuState)
        {
            case MenuState.PlayerTurnStart:
                BattleCommandInitialAppear();
                break;
            case MenuState.BattleCommandMenu:
                EnterBattleCommandMenu();
                break;
            case MenuState.Swapping:
                SwapTheMenus();
                break;
            case MenuState.ActionMenu:
                EnterSecondaryMenu();
                break;
            case MenuState.PlayerTurnEnd:
                ForceMenuToOriginalPositions();
                break;
        }
    }

    private async void BattleCommandInitialAppear()
    {
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        
        Tween tween = CreateTween();
        
        tween.TweenProperty(BattleCommandMenuContainer, "global_position:y", _targetMenuPositionY, MenuSwapTween)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);
        
        ChangeState(MenuState.BattleCommandMenu);
    }
    
    private void EnterBattleCommandMenu()
    {
        BattleCommandMenuContainer.Visible = true;
        ActionMenuContainer.Visible = false;
        
        GrabFocusOnButton(BattleCommandMenuContainer, InitialFocusButton);
    }
    
    private void EnterSecondaryMenu()
    {
        ActionMenuContainer.Visible = true;
        BattleCommandMenuContainer.Visible = false;
        
        SetMenuButtonsEnabled(ActionMenuContainer, false);
        
        _isCardKeyboardMode = false;
        _isActionMenuKeyboardMode = false;
    }
    
    private void SwapTheMenus()
    {
        Vector2 battleCommandMenuPos = BattleCommandMenuContainer.Position;
        Vector2 actionMenuPos = ActionMenuContainer.Position;
        
        if (_targetMenuState == MenuState.BattleCommandMenu) ActionMenuState?.Invoke(false);
        
        BattleCommandMenuContainer.Visible = true;
        ActionMenuContainer.Visible = true;

        Tween tween = CreateTween().SetParallel();
        
        MenuLerp(BattleCommandMenuContainer, actionMenuPos, MenuSwapTween, tween);
        MenuLerp(ActionMenuContainer, battleCommandMenuPos, MenuSwapTween, tween);

        tween.Finished += OnSwapMenuTweenFinished;
    }

    #endregion
    
    #region Private Signals

    private void OnSwapMenuTweenFinished()
    {
        if (_currentMenuState != MenuState.Swapping) return;
        if (_targetMenuState == MenuState.ActionMenu)
        {
            ActionMenuState?.Invoke(true);
        }

        (BattleCommandMenuContainer.ZIndex, ActionMenuContainer.ZIndex) =
        (ActionMenuContainer.ZIndex, BattleCommandMenuContainer.ZIndex);

        ChangeState(_targetMenuState);
    }
    
    private void OnAnyButtonPressed(TextureButton button)
    {
        if (_currentMenuState == MenuState.Swapping) return;
        
        ActionType action = button.Name.ToString() switch
        {
            "SwitchButton" => ActionType.Switch,
            "MovesButton"  => ActionType.Moves,
            "RunButton"    => ActionType.Run,
            "BlockButton"  => ActionType.Block,
            "AttackButton" => ActionType.Attack,
            "EvadeButton"  => ActionType.Evade,
            _ => ActionType.Moves
        };
        
        EmitSignal(SignalName.ActionSelected, (int)action);
        
        switch (_currentMenuState)
        {
            case MenuState.BattleCommandMenu when action == ActionType.Moves:
                _targetMenuState = MenuState.ActionMenu;
                ChangeState(MenuState.Swapping);
                break;
            case MenuState.ActionMenu when action == ActionType.Block:
                _targetMenuState = MenuState.BattleCommandMenu;
                ChangeState(MenuState.Swapping);
                break;
        }
    }
    
    private void OnAnyButtonHovered(TextureButton button)
    {
        switch (_currentMenuState)
        {
            case MenuState.BattleCommandMenu:
                _isBattleCommandMenuKeyboardMode = false;
                ReleaseFocusFromContainer(BattleCommandMenuContainer);
                break;
            case MenuState.ActionMenu:
                _isCardKeyboardMode = false;
                _isActionMenuKeyboardMode = false;
                
                // When the mouse hovers any action button, exit keyboard mode and ensure buttons are interactive.
                ReleaseFocusFromContainer(ActionMenuContainer);
                SetMenuButtonsEnabled(ActionMenuContainer, true);
                break;
        }
    }

    #endregion
    
    #region Helper Functions

    #region Initialization Helpers

    private void GetContainerChildren(Control container)
    {
        foreach (Node child in container.GetChildren())
        {
            if (child is TextureButton button)
            {
                button.Pressed += () => OnAnyButtonPressed(button);
                button.MouseEntered += () => OnAnyButtonHovered(button);
            }
        }
    }

    private void SetUpActionButtons()
    {
        _actionButtons = new List<TextureButton>();
        
        foreach (Node child in ActionMenuContainer.GetChildren())
        {
            if (child is TextureButton button) _actionButtons.Add(button);
        }
    }
    
    private void GetTargetMenuPosition()
    {
        Rect2 visibleRect = GetViewport().GetVisibleRect();
        float menuHeight = BattleCommandMenuContainer.GetRect().Size.Y;
        
        float screenBottomY = visibleRect.Position.Y + visibleRect.Size.Y;
        _targetMenuPositionY = screenBottomY - menuHeight;
    }
    
    private static void GrabFocusOnButton(Control container, string buttonName)
    {
        foreach (Node child in container.GetChildren())
        {
            if (child is TextureButton button && button.Name == buttonName)
            {
                button.GrabFocus();
            }
        }
    }
    
    private void ReleaseFocusFromContainer(Control container)
    {
        foreach (Node child in container.GetChildren())
        {
            if (child is TextureButton button && button.HasFocus())
            {
                button.ReleaseFocus();
            }
        }
    }

    #endregion

    #region Focus Helpers

    private void FocusOnAttackButton()
    {
        SetMenuButtonsEnabled(ActionMenuContainer, true);
        
        if (!_neighborsSetup)
        {
            SetupActionMenuFocusNeighbors();
            _neighborsSetup = true;
        }
        
        ReleaseFocusFromContainer(ActionMenuContainer);
        GrabFocusOnButton(ActionMenuContainer, ActionMenuDefaultFocus);
        _isActionMenuKeyboardMode = true;
        _isCardKeyboardMode = false;
    }
    
    private void SetupActionMenuFocusNeighbors()
    {
        if (_actionButtons.Count < 2) return;
        
        _actionButtons.Sort((a,b) => a.GlobalPosition.X.CompareTo(b.GlobalPosition.X));
        
        for (int i = 0; i < _actionButtons.Count; i++)
        {
            TextureButton button = _actionButtons[i];
            int leftIndex = WrapIndex(i - 1, _actionButtons.Count);
            int rightIndex = WrapIndex(i + 1, _actionButtons.Count);
            
            button.FocusNeighborLeft  = _actionButtons[leftIndex].GetPath();
            button.FocusNeighborRight = _actionButtons[rightIndex].GetPath();
            
            // ignore this
            button.FocusNeighborTop   = button.GetPath();
            button.FocusNeighborBottom = button.GetPath();
        }
    }
    
    private static int WrapIndex(int index, int count) => (index + count) % count;
    
    private TextureButton GetFocusedButton() => _actionButtons.FirstOrDefault(button => button.HasFocus());

    #endregion

    #region Input Handling

    private void HandleBattleCommandMenuInput()
    {
        if (!_isBattleCommandMenuKeyboardMode)
        {
            _isBattleCommandMenuKeyboardMode = true;
            GrabFocusOnButton(BattleCommandMenuContainer, InitialFocusButton);
        }
    }
    
    private void HandleActionMenuInput(UiSelection uiSelection)
    {
        if (_isCardKeyboardMode) return;
    
        if (uiSelection == UiSelection.Up)
        {
            SetMenuButtonsEnabled(ActionMenuContainer, false);
            MoveToCardSystem?.Invoke();
            return;
        }

        if (!_isActionMenuKeyboardMode)
        {
            FocusOnAttackButton();
        }
        
        if (uiSelection == UiSelection.Confirm && GetFocusedButton() is { } focused)
        {
            focused.EmitSignal(BaseButton.SignalName.Pressed);
        }
    }

    #endregion
    
    private void SetMenuButtonsEnabled(Control container, bool enabled)
    {
        foreach (Node child in container.GetChildren())
        {
            if (child is TextureButton button)
            {
                if (!enabled) button.ReleaseFocus();
                button.Disabled = !enabled;
            }
        }
    }
    
    private void MenuLerp(Control buttonContainer, Vector2 finalPos, float duration, Tween tween)
    {
        tween.TweenProperty(buttonContainer, "position", finalPos, duration)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);
    }
    #endregion
}