using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Ui;

namespace NeuralZeroProtocol.Scripts.Cards;

public partial class CardSelectionController
{
    public void OnCardClicked(Card clickedCard)
    {
        if (_state == CardSelectionState.SwapTheCards) return;
        if (!IsTopmostCard(clickedCard)) return;
        
        // Swap with center card
        if (clickedCard != CenterCard)
        {
            SwapWithCenter(clickedCard);
            return;
        }
        
        if (clickedCard != _selectedCard) SelectCard(clickedCard);
    }
    
    // Makes the given card the selected one. Deselects any previous selection.
    public void SelectCard(Card card)
    {
        if (_selectedCard == card) return;
        
        // Deselect previous card if there is one
        if (_selectedCard != null) ApplyVisualState(_selectedCard, false);
        
        Card oldCard = _selectedCard;
        _selectedCard = card;

        ChangeState(CardSelectionState.MouseMode);
        EmitSignal(SignalName.SelectionChanged, oldCard, card);
        
        // Apply visual effects for the new selected card
        ApplyVisualState(card, true);
    }
}