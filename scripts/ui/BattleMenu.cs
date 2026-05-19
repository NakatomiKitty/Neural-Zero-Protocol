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
	private Control _initialButtonContainer;
	private Control _secondaryButtonContainer;
	private MenuState _currentMenuState;
	private MenuState _targetMenuState;

	private Tween _menuTween;
	
	public override void _Ready()
	{
		_initialButtonContainer = GetNode<Control>("InitialButtonContainer");
		_secondaryButtonContainer = GetNode<Control>("SecondaryButtonContainer");
		
		GetContainerChildren(_initialButtonContainer);
		GetContainerChildren(_secondaryButtonContainer);
		
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
		_secondaryButtonContainer.Visible = false;
		
		// Set's the keyboard focus on the Moves Button
		GrabFocusOnButton(_initialButtonContainer, "MovesButton");
	}
	
	private void EnterSecondaryMenu()
	{
		// Hides the opposite container to prevent the keyboard accessing them
		_secondaryButtonContainer.Visible = true;
		_initialButtonContainer.Visible = false;
		
		// Set's the keyboard focus on the Attack Button
		GrabFocusOnButton(_secondaryButtonContainer, "AttackButton");
	}
	
	private void SwapTheMenus()
	{
		var initialBasePosition = _initialButtonContainer.Position;
		var secondaryBasePosition = _secondaryButtonContainer.Position;
		
		// Set's both of them to true while swapping
		_initialButtonContainer.Visible = true;
		_secondaryButtonContainer.Visible = true;
		
		// Swap the container's position with TWEEEEENNNNN
		
		SwapMenuLerp(_initialButtonContainer, secondaryBasePosition);
		SwapMenuLerp(_secondaryButtonContainer, initialBasePosition);

		_menuTween.Finished += () =>
		{
			if (_currentMenuState != MenuState.Swapping) return;
			
			// Swap Z Indexes
			(_initialButtonContainer.ZIndex, _secondaryButtonContainer.ZIndex) = 
				(_secondaryButtonContainer.ZIndex, _initialButtonContainer.ZIndex);
			
			ChangeState(_targetMenuState);
		};
	}
	private void OnAnyButtonPressed(TextureButton button)
	{
		if (_currentMenuState == MenuState.Swapping) return;
		
		var action = button.Name.ToString() switch
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
		
		EmitSignal(SignalName.ButtonSelected, (int)action); // Will be hooked up to the CombatStateMachine and CardSystem\
		
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

