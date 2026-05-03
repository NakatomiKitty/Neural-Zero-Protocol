using Godot;
using Godot.Collections;
using System;

namespace NeuralZeroProtocol.Scripts.Ui
{
	public enum UISelection
	{
		None,
		Left,
		Right,
		Up,
		Down,
		Confirm,
		Cancel
	};

	public partial class UISelectionController : Node
	{
		public UISelection GetUISelect()
		{
			if (Input.IsActionJustPressed("ui_left")) return UISelection.Left;
			if (Input.IsActionJustPressed("ui_right")) return UISelection.Right;
			if (Input.IsActionJustPressed("ui_up")) return UISelection.Up;
			if (Input.IsActionJustPressed("ui_down")) return UISelection.Down;
			if (Input.IsActionJustPressed("ui_select")) return UISelection.Confirm;
			if (Input.IsActionJustPressed("ui_cancel")) return UISelection.Cancel;
			return UISelection.None;
		}
	}
}
