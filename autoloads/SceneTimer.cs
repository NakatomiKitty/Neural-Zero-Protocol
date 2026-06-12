using Godot;

namespace NeuralZeroProtocol.Autoloads;

public partial class SceneTimer : Node
{
	private float _startTime;
	private bool _tracking;

	public override void _Ready()
	{
		GetTree().NodeAdded += OnNodeAdded;
	}
	public void StartTimer()
	{
		_startTime = Time.GetTicksMsec();
		_tracking = true;
	}

	private void OnNodeAdded(Node node)
	{
		if (_tracking)
		{
			float elapsedTime = Time.GetTicksMsec() - _startTime;
			GD.Print($"Scene swap took: {elapsedTime} ms");
			_tracking = false;
		}
	}
}
