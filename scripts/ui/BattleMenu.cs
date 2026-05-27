using System;
using Godot;

namespace NeuralZeroProtocol.Scripts.Ui;

public enum ActionType 
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
	[Signal] public delegate void ActionSelectedEventHandler(int actionType); // Rename to ActionType once replaced ActionPanel
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

		switch (uiSelection)
		{
			case UiSelection.Up:
				DisableActionMenuButtons(true);
				EmitSignal(SignalName.MoveToCardSystem);
				break;
		}
	}
	
	public void OnKeyboardModeCancelled(bool isCancelled) 
	{
		if (_currentMenuState != MenuState.ActionMenu) return;
		
		DisableActionMenuButtons(false);
		
		if (isCancelled) GrabFocusOnButton(ActionMenuContainer, "AttackButton");
	}

	public void OnAnyButtonHovered()
	{
		DisableActionMenuButtons(false);
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
		
		ActionType action = button.Name.ToString() switch
		{
			"SwitchButton" => ActionType.Switch,
			"MovesButton" => ActionType.Moves,
			"RunButton" => ActionType.Run,
			"BlockButton" => ActionType.Block,
			"AttackButton" => ActionType.Attack,
			"EvadeButton" => ActionType.Evade,
			_ => ActionType.Moves
		};
		
		EmitSignal(SignalName.ActionSelected, (int)action); // Will be hooked up to the CombatStateMachine and CardSystem
		
		switch (_currentMenuState)
		{
			case MenuState.InitialMenu when action == ActionType.Moves:
				_targetMenuState = MenuState.ActionMenu;
				ChangeState(MenuState.Swapping);
				break;
			// Placeholder, will be adding a back button
			case MenuState.ActionMenu when action == ActionType.Block:
				_targetMenuState = MenuState.InitialMenu;
				ChangeState(MenuState.Swapping);
				break;
		}
	}

	#region Helper functions

	private void DisableActionMenuButtons(bool isTrue)
	{
		foreach (Node child in ActionMenuContainer.GetChildren())
		{
			if (child is TextureButton button)
			{
				button.ReleaseFocus();
				button.Disabled = isTrue;
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
				button.MouseEntered += OnAnyButtonHovered;
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

