using Godot;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Cards;

[Scene]
public partial class CardBorderController : Node
{
	[Node("CardBorder")] private Sprite2D _cardBorder;
	
	private static readonly Vector2 CardBorderSubtract = new(.7f, .7f);
	private const float CardBorderTweenDuration = 0.15f;
	private const float CardBorderFadeDuration = 0.10f;
	private const float MaxCardScale = 0.45f;

	private Vector2 _cardBorderOriginalScale;
	
	public override void _Notification(int what)
	{
		if (what == NotificationSceneInstantiated) WireNodes();
	}
	
	public override void _Ready()
	{
		_cardBorder.Visible = true;
		_cardBorderOriginalScale = _cardBorder.Scale;
	}
	
	public void SetCenterCard(Card centerCard) => _cardBorder.GlobalPosition = centerCard.GlobalPosition;
	
	public async void OnEnterCardSelection(Card selectedCard)
	{
		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
        
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(_cardBorder, "modulate:a", 1.0f, CardBorderFadeDuration);

		OnApplyBorderEffect(selectedCard, selectedCard.Scale);
	}

	public void OnExitCardSelection()
	{
		Tween tween = CreateTween().SetParallel();
		tween.TweenProperty(_cardBorder, "modulate:a", 0.0f, CardBorderFadeDuration);
	}

	public void OnApplyBorderEffect(Card card, Vector2 cardScale)
	{
		Vector2 newCardBorderScale = cardScale - CardBorderSubtract;

		float cardBorderClampedX = Mathf.Clamp(newCardBorderScale.X, _cardBorderOriginalScale.X, MaxCardScale);
		float cardBorderClampedY = Mathf.Clamp(newCardBorderScale.Y, _cardBorderOriginalScale.Y, MaxCardScale);

		Tween tween = CreateTween().SetParallel();

		tween.TweenProperty(_cardBorder, "global_rotation", card.GlobalRotation, CardBorderTweenDuration);

		tween.TweenProperty(_cardBorder, "global_position", card.GlobalPosition, CardBorderTweenDuration)
			.SetTrans(Tween.TransitionType.Quint)
			.SetEase(Tween.EaseType.Out);

		tween.TweenProperty(_cardBorder, "scale", new Vector2(cardBorderClampedX, cardBorderClampedY), CardBorderTweenDuration);
	}
}
