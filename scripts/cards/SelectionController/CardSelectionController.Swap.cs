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
        Vector2 clickedBase = _cardSystem.CardBasePositions[clickedCard];
        Vector2 centerBase = _cardSystem.CardBasePositions[oldCenter];
        
        KillPositionTween(clickedCard);
        KillPositionTween(oldCenter);

        // Tween shit
        
        _swapCardTween = CreateTween();
        
        _swapCardTween.Parallel().TweenProperty(clickedCard, "position", centerBase, SwapTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.InOut);
        _swapCardTween.Parallel().TweenProperty(clickedCard, "rotation", oldCenter.Rotation, SwapTweenDuration);
        
        _swapCardTween.Parallel().TweenProperty(oldCenter, "position", clickedBase, SwapTweenDuration)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.InOut);
        
        _swapCardTween.Parallel().TweenProperty(oldCenter, "rotation", clickedCard.Rotation, SwapTweenDuration);
        
        _swapCardTween.Finished += () => OnSwapCardTweenFinished(clickedCard, oldCenter, clickedBase, centerBase);
    }

    private void OnSwapCardTweenFinished(Card clickedCard, Card oldCenter, Vector2 clickedBase, Vector2 centerBase)
    {
        if (_state != CardSelectionState.SwapTheCards) return;

        // Swap base positions
        (_cardSystem.CardBasePositions[clickedCard], _cardSystem.CardBasePositions[oldCenter]) =
            (centerBase, clickedBase);

        // Swap Z indexes
        (_cardSystem.OriginalZIndexes[clickedCard], _cardSystem.OriginalZIndexes[oldCenter]) =
            (_cardSystem.OriginalZIndexes[oldCenter], _cardSystem.OriginalZIndexes[clickedCard]);

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

        EmitSignal(SignalName.GetKeyboardHoveredCard, CenterCard);

        clickedCard.UpdatePriority();
        oldCenter.UpdatePriority();

        ChangeState(CardSelectionState.MouseMode);
    }
}