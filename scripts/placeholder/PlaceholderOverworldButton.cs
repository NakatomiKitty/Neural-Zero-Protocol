using Godot;
using System;
using NeuralZeroProtocol.Autoloads;

public partial class PlaceholderOverworldButton : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Connect(SignalName.Pressed, Callable.From(OnButtonPressed));
	}

	private void OnButtonPressed()
	{
		SceneTimer sceneTimer = GetNode<SceneTimer>("/root/SceneTimer");
		
		sceneTimer.StartTimer();
		GetTree().ChangeSceneToFile("res://scenes/battle_scene.tscn");
	}
}
