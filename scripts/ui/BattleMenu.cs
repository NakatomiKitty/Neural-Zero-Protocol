using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace NeuralZeroProtocol.Scripts.Ui;

public enum ActionType { Switch, Moves, Run, Block, Attack, Evade }
public enum MenuState { InitialMenu, Swapping, ActionMenu }

public partial class BattleMenu : Control
{
    [Signal] public delegate void ActionSelectedEventHandler(int actionType);
    public event Action<bool> ActionMenuState;
    public event Action MoveToCardSystem;
    
    public Control ActionMenuContainer;
    public Control InitialMenuContainer;
    private Vector2 _originalActionMenuPosition;
    private Vector2 _originalInitialMenuPosition;
    
    private const float MenuSwapTween = 0.5f;
    private const float MenuForceExitTween = 2f;
    private Tween _menuTween;
    
    private const string InitialFocusButton = "MovesButton";
    private const string ActionMenuDefaultFocus = "AttackButton";
    
    private MenuState _currentMenuState;
    private MenuState _targetMenuState;
    
    private List<TextureButton> _actionButtons;
    
    private bool _isInitialMenuKeyboardMode;    // InitialMenu keyboard mode
    private bool _isActionMenuKeyboardMode;     // ActionMenu keyboard mode
    private bool _isCardKeyboardMode;
    
    private bool _neighborsSetup;
    
    public override void _Ready()
    {
        InitialMenuContainer = GetNode<Control>("InitialButtonContainer");
        ActionMenuContainer = GetNode<Control>("SecondaryButtonContainer");
        _originalActionMenuPosition = ActionMenuContainer.GlobalPosition;
        _originalInitialMenuPosition = InitialMenuContainer.GlobalPosition;
        
        GetContainerChildren(InitialMenuContainer);
        GetContainerChildren(ActionMenuContainer);
        
        _actionButtons = new List<TextureButton>();
        
        foreach (Node child in ActionMenuContainer.GetChildren())
        {
            if (child is TextureButton button)
            {
                _actionButtons.Add(button);
            }
        }
        
        ChangeState(MenuState.InitialMenu);
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
        
        if (_currentMenuState == MenuState.InitialMenu)
        {
            _isInitialMenuKeyboardMode = false;
            
            ReleaseFocusFromContainer(InitialMenuContainer);
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
            case MenuState.InitialMenu:
                HandleInitialMenuInput();
                break;
            case MenuState.ActionMenu:
                HandleActionMenuInput(uiSelection);
                break;
        }
    }

    public void ForceMenuToOriginalPositions()
    {
        _menuTween?.Kill();
        
        MenuLerp(InitialMenuContainer, _originalInitialMenuPosition, MenuForceExitTween);
        MenuLerp(ActionMenuContainer, _originalActionMenuPosition, MenuForceExitTween);
    }
    #endregion

    #region State Machine and States

    private void ChangeState(MenuState newState)
    {
        _currentMenuState = newState;
        
        switch (_currentMenuState)
        {
            case MenuState.InitialMenu:
                EnterInitialMenu();
                break;
            case MenuState.Swapping:
                SwapTheMenus();
                break;
            case MenuState.ActionMenu:
                EnterSecondaryMenu();
                break;
        }
    }
    
    private void EnterInitialMenu()
    {
        InitialMenuContainer.Visible = true;
        ActionMenuContainer.Visible = false;
        
        GrabFocusOnButton(InitialMenuContainer, InitialFocusButton);
    }
    
    private void EnterSecondaryMenu()
    {
        ActionMenuContainer.Visible = true;
        InitialMenuContainer.Visible = false;
        
        SetMenuButtonsEnabled(ActionMenuContainer, false);
        
        _isCardKeyboardMode = false;
        _isActionMenuKeyboardMode = false;
    }
    
    private void SwapTheMenus()
    {
        Vector2 initialMenuPos = InitialMenuContainer.Position;
        Vector2 actionMenuPos = ActionMenuContainer.Position;

        _menuTween?.Kill();
        
        if (_targetMenuState == MenuState.InitialMenu)
        {
            ActionMenuState?.Invoke(false);
        }
        
        InitialMenuContainer.Visible = true;
        ActionMenuContainer.Visible = true;
        
        MenuLerp(InitialMenuContainer, actionMenuPos, MenuSwapTween);
        MenuLerp(ActionMenuContainer, initialMenuPos, MenuSwapTween);

        _menuTween.Finished += OnSwapMenuTweenFinished;
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

        (InitialMenuContainer.ZIndex, ActionMenuContainer.ZIndex) =
        (ActionMenuContainer.ZIndex, InitialMenuContainer.ZIndex);

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
            case MenuState.InitialMenu when action == ActionType.Moves:
                _targetMenuState = MenuState.ActionMenu;
                ChangeState(MenuState.Swapping);
                break;
            case MenuState.ActionMenu when action == ActionType.Block:
                _targetMenuState = MenuState.InitialMenu;
                ChangeState(MenuState.Swapping);
                break;
        }
    }
    
    private void OnAnyButtonHovered(TextureButton button)
    {
        switch (_currentMenuState)
        {
            case MenuState.InitialMenu:
                _isInitialMenuKeyboardMode = false;
                ReleaseFocusFromContainer(InitialMenuContainer);
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

    private void HandleInitialMenuInput()
    {
        if (!_isInitialMenuKeyboardMode)
        {
            _isInitialMenuKeyboardMode = true;
            GrabFocusOnButton(InitialMenuContainer, InitialFocusButton);
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
    
    private void MenuLerp(Control buttonContainer, Vector2 finalPos, float duration)
    {
        _menuTween = CreateTween();
        
        _menuTween.TweenProperty(buttonContainer, "position", finalPos, duration)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);
    }
    
    
    #endregion
    
}