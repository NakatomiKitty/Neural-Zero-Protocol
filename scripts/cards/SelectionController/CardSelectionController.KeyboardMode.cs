using Godot;
using NeuralZeroProtocol.Scripts.Ui;

namespace NeuralZeroProtocol.Scripts.Cards;

public partial class CardSelectionController
{
    public void GetUiInput(UiSelection uiSelection)
    {
        if (_state == CardSelectionState.SwapTheCards) return;
        
        if (_isInActionMenu) return;

        switch (uiSelection)
        {
            case UiSelection.Left:
            case UiSelection.Right:
            {
                if (_state != CardSelectionState.KeyboardMode)
                    ActivateKeyboardMode();
                if (uiSelection == UiSelection.Left)
                    KeyboardMoveLeft();
                else
                    KeyboardMoveRight();
                return;
            }
            case UiSelection.Confirm when _state == CardSelectionState.KeyboardMode:
                ConfirmCard();
                break;
            case UiSelection.Cancel:
                EmitSignal(SignalName.KeyboardModeCancelled, true);
                _isInActionMenu = true;
                DeactivateKeyboardMode();
                ChangeState(CardSelectionState.Idle);
                break;
        }
    }
    
    public void KeyboardMoveLeft()
    {
        if (CardHand == null || CardHand.Count == 0) return;

        int centerIndex = CardHand.IndexOf(_keyboardHoveredCard);

        int toLeftCardIndex = centerIndex - 1;

        if (toLeftCardIndex < 0) toLeftCardIndex = CardHand.Count - 1;

        _keyboardHoveredCard = CardHand[toLeftCardIndex];

        EmitSignal(SignalName.GetKeyboardHoveredCard, _keyboardHoveredCard);
    }
    
    public void KeyboardMoveRight()
    {
        if (CardHand == null || CardHand.Count == 0) return;

        int centerIndex = CardHand.IndexOf(_keyboardHoveredCard);

        int toRightCardIndex = centerIndex + 1;

        if(toRightCardIndex >= CardHand.Count) toRightCardIndex = 0;

        _keyboardHoveredCard = CardHand[toRightCardIndex];

        EmitSignal(SignalName.GetKeyboardHoveredCard, _keyboardHoveredCard);
    }

    public void ConfirmCard()
    {
        if (_state != CardSelectionState.KeyboardMode) return;
        
        if (CardHand == null || CardHand.Count == 0) return;
        
        if (_keyboardHoveredCard == null) return; 
        
        if (_keyboardHoveredCard == CenterCard) return;

        // Trigger the same swap logic as if the card was clicked
        SwapWithCenter(_keyboardHoveredCard);
    }

    public void OnMoveToCardSystem()
    {
        if (_state == CardSelectionState.KeyboardMode) return;
        
        _isInActionMenu = false;
        ActivateKeyboardMode();
    }
    
    // Helper Functions
    private void ActivateKeyboardMode()
    {
        // Start keyboard highlight from the current center card
        _keyboardHoveredCard = CenterCard;
        EmitSignal(SignalName.GetKeyboardHoveredCard, _keyboardHoveredCard);
        ChangeState(CardSelectionState.KeyboardMode);
    }
    
    private void DeactivateKeyboardMode()
    {
        if (_state != CardSelectionState.KeyboardMode) return;
        
        EmitSignal(SignalName.KeyboardModeDeactivated);
        
        ChangeState(CardSelectionState.Idle);
        _keyboardHoveredCard = null;
    }
}