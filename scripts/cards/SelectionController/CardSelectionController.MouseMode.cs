namespace NeuralZeroProtocol.Scripts.Cards;

public partial class CardSelectionController
{
    public void OnCardClicked(Card clickedCard)
    {
        if (_state == CardSelectionState.SwapTheCards) return;
        if (!IsTopmostCard(clickedCard)) return;
        
        // Swap with center card
        if (clickedCard != _centerCard)
        {
            SwapWithCenter(clickedCard);
            return;
        }
        
        if (clickedCard != _selectedCard) SelectCard(clickedCard);
    }
    
    // Makes the given card the selected one. Deselects any previous selection.
    private void SelectCard(Card card)
    {
        // Deselect previous card if there is one
        if (_selectedCard != null) ApplyVisualState(_selectedCard, false);
        
        Card oldCard = _selectedCard;
        _selectedCard = card;

        ChangeState(CardSelectionState.MouseMode);
        SelectionChanged?.Invoke(oldCard, card);
        
        // Apply visual effects for the new selected card
        ApplyVisualState(card, true);
    }
}