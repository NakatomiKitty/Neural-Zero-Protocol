using System;
using Godot;

namespace NeuralZeroProtocol.Scripts.Ui;

public enum ButtonType // Rename to ActionType once replaced ActionPanel
{
    Switch,
    Moves,
    Run,
    Block,
    Attack,
    Evade
}

public enum MenuState // TODO: ADD A STATE WHERE ON START ROUND OR ON NEXT TURN, THE INITIAL MENU APPEARS!
{
	InitialMenu,
	Swapping,
	ActionMenu
}
public partial class BattleMenu : Control
{
	private const float MenuSwapTween = 0.5f;
	
	[Signal] public delegate void MenuStateChangedEventHandler(MenuState newState);
	[Signal] public delegate void ButtonSelectedEventHandler(int actionType); // Rename to ActionType once replaced ActionPanel
	[Signal] public delegate void ActionMenuStateEventHandler(bool isTrue);
	[Signal] public delegate void MoveToCardSystemEventHandler();
	
	private MenuState _currentMenuState;
	private MenuState _targetMenuState;

	private Tween _menuTween;
	
	public Control InitialButtonContainer;
	public Control ActionMenuContainer;
	
	public override void _Ready()
	{
		InitialButtonContainer = GetNode<Control>("InitialButtonContainer");
		ActionMenuContainer = GetNode<Control>("SecondaryButtonContainer");
		
		GetContainerChildren(InitialButtonContainer);
		GetContainerChildren(ActionMenuContainer);
		
		ChangeState(MenuState.InitialMenu);
	}

	public void GetUiInput(UiSelection uiSelection)
	{
		if (_currentMenuState != MenuState.ActionMenu) return;

		if (uiSelection == UiSelection.Up)
		{
			GD.Print("should disable action menu buttons");
			DisableActionMenuButtons(true);
			EmitSignal(SignalName.MoveToCardSystem);
		}
	}

	public void OnKeyboardModeCancelled(bool isCancelledByMouse)
	{
		if (_currentMenuState != MenuState.ActionMenu) return;
		
		DisableActionMenuButtons(false);
		
		if (!isCancelledByMouse) GrabFocusOnButton(ActionMenuContainer, "AttackButton");
	}
	
	public void ChangeState(MenuState newState)
	{
		_currentMenuState = newState;
		EmitSignal(SignalName.MenuStateChanged, (int)newState);

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
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	private void EnterInitialMenu()
	{
		// Hides the opposite container to prevent the keyboard accessing them
		InitialButtonContainer.Visible = true;
		ActionMenuContainer.Visible = false;
		
		// Set's the keyboard focus on the Moves Button
		GrabFocusOnButton(InitialButtonContainer, "MovesButton");
	}
	
	private void EnterSecondaryMenu()
	{
		// Hides the opposite container to prevent the keyboard accessing them
		ActionMenuContainer.Visible = true;
		InitialButtonContainer.Visible = false;
		
		DisableActionMenuButtons(true);
	}
	
	private void SwapTheMenus()
	{
		
		Vector2 initialBasePosition = InitialButtonContainer.Position;
		Vector2 secondaryBasePosition = ActionMenuContainer.Position;
		
		if (_targetMenuState == MenuState.InitialMenu)
		{
			EmitSignal(SignalName.ActionMenuState, false);
		}
		
		// Set's both of them to true while swapping
		InitialButtonContainer.Visible = true;
		ActionMenuContainer.Visible = true;
		
		// Swap the container's position with TWEEEEENNNNN
		
		SwapMenuLerp(InitialButtonContainer, secondaryBasePosition);
		SwapMenuLerp(ActionMenuContainer, initialBasePosition);

		_menuTween.Finished += () =>
		{
			if (_currentMenuState != MenuState.Swapping) return;
			
			if (_targetMenuState == MenuState.ActionMenu)
			{
				EmitSignal(SignalName.ActionMenuState, true);
			}
			
			// Swap Z Indexes
			(InitialButtonContainer.ZIndex, ActionMenuContainer.ZIndex) = 
			(ActionMenuContainer.ZIndex, InitialButtonContainer.ZIndex);
			
			ChangeState(_targetMenuState);
		};
	}
	private void OnAnyButtonPressed(TextureButton button)
	{
		if (_currentMenuState == MenuState.Swapping) return;
		
		ButtonType action = button.Name.ToString() switch
		{
			"SwitchButton" => ButtonType.Switch,
			"MovesButton" => ButtonType.Moves,
			"RunButton" => ButtonType.Run,
			"BlockButton" => ButtonType.Block,
			"AttackButton" => ButtonType.Attack,
			"EvadeButton" => ButtonType.Evade,
			_ => ButtonType.Moves
		};
		
		GD.Print(action);
		
		EmitSignal(SignalName.ButtonSelected, (int)action); // Will be hooked up to the CombatStateMachine and CardSystem
		
		if (_currentMenuState == MenuState.InitialMenu && action == ButtonType.Moves)
		{
			_targetMenuState = MenuState.ActionMenu;
			ChangeState(MenuState.Swapping);
		}
		
		else if (_currentMenuState == MenuState.ActionMenu && action == ButtonType.Block)
		{
			_targetMenuState = MenuState.InitialMenu;
			ChangeState(MenuState.Swapping);
		}
	}

	#region Helper functions

	private void DisableActionMenuButtons(bool isTrue)
	{
		foreach (Node child in ActionMenuContainer.GetChildren())
		{
			if (child is TextureButton button)
			{
				button.Disabled = isTrue;
				button.ReleaseFocus();
			}
		}
	}
	private void GetContainerChildren(Control buttonContainer)
	{
		foreach (Node child in buttonContainer.GetChildren())
		{
			if (child is TextureButton button)
			{
				button.Pressed += () => OnAnyButtonPressed(button);
			}
		}
	}

	private void SwapMenuLerp(Control buttonContainer, Vector2 finalPosition)
	{
		_menuTween = CreateTween();

		_menuTween.TweenProperty(buttonContainer, "position", finalPosition, MenuSwapTween)
			.SetTrans(Tween.TransitionType.Quint)
			.SetEase(Tween.EaseType.Out);
	}
	
	private static void GrabFocusOnButton(Control container, string buttonName)
	{
		foreach (Node child in container.GetChildren())
		{
			if (child is TextureButton button && button.Name == buttonName)
			{
				button.GrabFocus();
				break;
			}
		}
	}
	#endregion 
}

