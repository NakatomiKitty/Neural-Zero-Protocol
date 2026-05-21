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
	SecondaryMenu
}
public partial class BattleMenu : Control
{
	private const float MenuSwapTween = 0.5f;
	
	[Signal] public delegate void MenuStateChangedEventHandler(MenuState newState);
	[Signal] public delegate void ButtonSelectedEventHandler(int actionType); // Rename to ActionType once replaced ActionPanel
	[Signal] public delegate void ActionMenuStateEventHandler(bool isTrue);
	
	private MenuState _currentMenuState;
	private MenuState _targetMenuState;
	private Control _initialButtonContainer;

	private Tween _menuTween;
	
	public Control ActionMenuContainer;
	
	public override void _Ready()
	{
		_initialButtonContainer = GetNode<Control>("InitialButtonContainer");
		ActionMenuContainer = GetNode<Control>("SecondaryButtonContainer");
		
		GetContainerChildren(_initialButtonContainer);
		GetContainerChildren(ActionMenuContainer);
		
		ChangeState(MenuState.InitialMenu);
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
			case MenuState.SecondaryMenu:
				EnterSecondaryMenu();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	private void EnterInitialMenu()
	{
		// Hides the opposite container to prevent the keyboard accessing them
		_initialButtonContainer.Visible = true;
		ActionMenuContainer.Visible = false;
		
		// Set's the keyboard focus on the Moves Button
		GrabFocusOnButton(_initialButtonContainer, "MovesButton");
	}
	
	private void EnterSecondaryMenu()
	{
		// Hides the opposite container to prevent the keyboard accessing them
		ActionMenuContainer.Visible = true;
		_initialButtonContainer.Visible = false;
		
		// Set's the keyboard focus on the Attack Button
		GrabFocusOnButton(ActionMenuContainer, "AttackButton");
	}
	
	private void SwapTheMenus()
	{
		Vector2 initialBasePosition = _initialButtonContainer.Position;
		Vector2 secondaryBasePosition = ActionMenuContainer.Position;
		
		// Set's both of them to true while swapping
		_initialButtonContainer.Visible = true;
		ActionMenuContainer.Visible = true;
		
		// Swap the container's position with TWEEEEENNNNN
		
		SwapMenuLerp(_initialButtonContainer, secondaryBasePosition);
		SwapMenuLerp(ActionMenuContainer, initialBasePosition);

		_menuTween.Finished += () =>
		{
			if (_currentMenuState != MenuState.Swapping) return;

			EmitSignal(SignalName.ActionMenuState, _targetMenuState == MenuState.SecondaryMenu);

			// Swap Z Indexes
			(_initialButtonContainer.ZIndex, ActionMenuContainer.ZIndex) = 
				(ActionMenuContainer.ZIndex, _initialButtonContainer.ZIndex);
			
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
			_targetMenuState = MenuState.SecondaryMenu;
			ChangeState(MenuState.Swapping);
		}
		
		else if (_currentMenuState == MenuState.SecondaryMenu && action == ButtonType.Block)
		{
			_targetMenuState = MenuState.InitialMenu;
			ChangeState(MenuState.Swapping);
		}
	}

	#region Helper functions
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

