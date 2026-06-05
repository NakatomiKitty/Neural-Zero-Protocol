using Godot;

namespace NeuralZeroProtocol.Scripts.Ui;

public enum UiSelection
{
	None,
	Left,
	Right,
	Up,
	Down,
	Confirm,
	Cancel
}

public partial class UiSelectionController : Node
{
	public static UiSelection GetUiSelect()
	{
		if (Input.IsActionJustPressed("ui_left")) return UiSelection.Left;
		if (Input.IsActionJustPressed("ui_right")) return UiSelection.Right;
		if (Input.IsActionJustPressed("ui_up")) return UiSelection.Up;
		if (Input.IsActionJustPressed("ui_down")) return UiSelection.Down;
		if (Input.IsActionJustPressed("ui_select")) return UiSelection.Confirm;
		if (Input.IsActionJustPressed("ui_cancel")) return UiSelection.Cancel;
		return UiSelection.None;
	}
}

