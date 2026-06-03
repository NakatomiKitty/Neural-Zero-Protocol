using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Ui;

namespace NeuralZeroProtocol.Scripts.Cards;

public partial class CardSelectionController
{
    private void SwapWithCenter(Card clickedCard)
    {
        if (_state == CardSelectionState.SwapTheCards) return;
        ChangeState(CardSelectionState.SwapTheCards);

        _swapCardTween?.Kill();

        Card oldCenter = CenterCard;
        Vector2 clickedBase = CardBasePositions[clickedCard];
        Vector2 centerBase = CardBasePositions[oldCenter];
        
        KillPositionTween(clickedCard);
        KillPositionTween(oldCenter);

        // Tween shit
        
        _swapCardTween = CreateTween().SetParallel();
        
        _swapCardTween.TweenProperty(clickedCard, "position", centerBase, SwapTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.InOut);
        _swapCardTween.TweenProperty(clickedCard, "rotation", oldCenter.Rotation, SwapTweenDuration);
        
        _swapCardTween.TweenProperty(oldCenter, "position", clickedBase, SwapTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.InOut);
        
        _swapCardTween.TweenProperty(oldCenter, "rotation", clickedCard.Rotation, SwapTweenDuration);
        
        _swapCardTween.Finished += () => OnSwapCardTweenFinished(clickedCard, oldCenter, clickedBase, centerBase);
    }

    private void OnSwapCardTweenFinished(Card clickedCard, Card oldCenter, Vector2 clickedBase, Vector2 centerBase)
    {
        if (_state != CardSelectionState.SwapTheCards) return;

        // Swap base positions
        (CardBasePositions[clickedCard], CardBasePositions[oldCenter]) =
            (centerBase, clickedBase);

        // Swap Z indexes
        (OriginalZIndexes[clickedCard], OriginalZIndexes[oldCenter]) =
        (OriginalZIndexes[oldCenter], OriginalZIndexes[clickedCard]);

        int clickedIndex = CardHand.IndexOf(clickedCard);
        int centerIndex = CardHand.IndexOf(oldCenter);
        if (clickedIndex != -1 && centerIndex != -1)
        {
            CardHand[clickedIndex] = oldCenter;
            CardHand[centerIndex] = clickedCard;
        }

        // First, deselect the current centerCard
        DeselectCard();
        // Then select the new centerCard
        SelectCard(clickedCard);

        // Don't forget to update the _centerCard value to the new _centerCard!
        CenterCard = clickedCard;
        
        EmitSignal(SignalName.CurrentSelectedCardChanged, CenterCard);
        
        EmitSignal(SignalName.KeyboardHoveredCardChanged, CenterCard);

        clickedCard.UpdatePriority();
        oldCenter.UpdatePriority();

        ChangeState(CardSelectionState.MouseMode);
    }
}