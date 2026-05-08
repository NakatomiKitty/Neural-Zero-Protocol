using Godot;

namespace NeuralZeroProtocol.Scripts.Ui
{
	public enum ButtonType // Rename to ActionType once replaced ActionPanel
    {
        Switch,
        Moves,
        Run,
        Block,
        Attack,
        Evade
    }
	public partial class BattleMenu : Control
	{
		private const float MENU_SWAP_TWEEN = 0.5f;
		
		[Signal] public delegate void ButtonSelectedEventHandler(int actionType); // Rename to ActionType once replaced ActionPanel
		private Control _initialButtonContainer;
		private Control _secondaryButtonContainer;
		
		private Tween _initialContainerTween, _secondaryContainerTween;

		private bool _isSwapping;
		public override void _Ready()
		{
			InitializeChildren();
			
			// Hide SecondaryButtonContainer at first
			GetFirstContainerChildren();
			GetSecondContainerChildren();

			ButtonSelected += OnMovesButtonSelected;

		}

		private void InitializeChildren()
		{
			_initialButtonContainer = GetNode<Control>("InitialButtonContainer");
			_secondaryButtonContainer = GetNode<Control>("SecondaryButtonContainer");
		}

		private void GetFirstContainerChildren()
		{
			foreach (Node child in _initialButtonContainer.GetChildren())
			{
				if (child is TextureButton button)
				{
					button.Pressed += () => OnAnyButtonPressed(button);
				}
			}
		}
		
		private void GetSecondContainerChildren()
		{
			foreach (Node child in _secondaryButtonContainer.GetChildren())
			{
				if (child is TextureButton button)
				{
					
					button.Pressed += () => OnAnyButtonPressed(button);
				}
			}
		}

		private void OnAnyButtonPressed(TextureButton button)
		{
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
			EmitSignal(SignalName.ButtonSelected, (int)action);
		}
		
		private void OnMovesButtonSelected(int actionType)
		{
			if (_isSwapping) return;
			ButtonType action = (ButtonType)actionType;
			
			if (action == ButtonType.Moves)
			{
				SwapMenus();
			}
		}

		private void SwapMenus()
		{
			var initialBasePos = _initialButtonContainer.Position;
			var secondaryBasePos = _secondaryButtonContainer.Position;

			_isSwapping = true;
			
			_initialContainerTween = CreateTween();
			_secondaryContainerTween = CreateTween();
			
			// Swap the container's position with TWEEEEENNNNN
			_initialContainerTween.TweenProperty(_initialButtonContainer, "position", secondaryBasePos, MENU_SWAP_TWEEN)
				.SetTrans(Tween.TransitionType.Quint)
				.SetEase(Tween.EaseType.Out);
				
			_secondaryContainerTween.TweenProperty(_secondaryButtonContainer, "position", initialBasePos, MENU_SWAP_TWEEN)
				.SetTrans(Tween.TransitionType.Quint)
				.SetEase(Tween.EaseType.Out);

			_initialContainerTween.Finished += () =>
			{
				// Swap Z Indexes
				(_initialButtonContainer.ZIndex, _secondaryButtonContainer.ZIndex) =
					(_secondaryButtonContainer.ZIndex, _initialButtonContainer.ZIndex);
				
				_isSwapping = false;
			};
		}
	}
}

