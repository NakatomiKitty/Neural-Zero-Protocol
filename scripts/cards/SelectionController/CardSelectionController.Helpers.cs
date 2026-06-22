using Godot;
using Godot.Collections;

namespace NeuralZeroProtocol.Scripts.Cards;

public partial class CardSelectionController
{
    private void ApplyVisualState(Card card, bool selected)
    {
        KillPositionTween(card);
        
        Tween tween = CreateTween();
        Vector2 basePosition = CardBasePositions[card];
        Vector2 targetScale = selected ? Vector2.One * SelectedCardSizeMultiplier : Vector2.One;
        Vector2 targetPosition = selected ? basePosition + Vector2.Down * SelectedCardVerticalOffset : basePosition;

        tween.Parallel()
            .TweenProperty(card, "scale", targetScale, SelectionTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
        
        tween.Parallel()
            .TweenProperty(card, "position", targetPosition, SelectionTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        _activePositionTweens[card] = tween;
        
        card.ZIndex = selected ? CardSystem.SelectedZ : OriginalZIndexes[card];
        
        card.UpdatePriority();
    }
    
    private bool IsTopmostCard(Card clickedCard)
    {
        Card highestCard = null;
        int highestZ = int.MinValue;
        
        foreach (Dictionary result in MouseQuery())
        {
            if (result["collider"].As<Area2D>() is Area2D area && area.GetParent<Card>() is Card card && card.ZIndex > highestZ)
            {
                highestCard = card;
                highestZ = card.ZIndex;
            }
        }
        
        return highestCard == clickedCard;
    }
    
    private bool IsAnyCardUnderMouse()
    {
        foreach (Dictionary result in MouseQuery())
        {
            if (result["collider"].As<Area2D>()?.GetParent<Card>() is Card)
                return true;
        }
        return false;
    }

    private Array<Dictionary> MouseQuery()
    {
        Vector2 mousePos = GetGlobalMousePosition();
        PhysicsDirectSpaceState2D space = GetWorld2D().DirectSpaceState;
        PhysicsPointQueryParameters2D query = new()
        {
            Position = mousePos,
            CollideWithAreas = true,
            CollisionMask = 1
        };

        Array<Dictionary> results = space.IntersectPoint(query);
        return results;
    }
    
    private void KillPositionTween(Card card)
    {
        if (_activePositionTweens.TryGetValue(card, out Tween tween) && tween.IsRunning())
            tween.Kill();
        _activePositionTweens.Remove(card);
    }
}