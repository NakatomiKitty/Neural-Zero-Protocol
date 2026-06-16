using Godot;
using NeuralZeroProtocol.Scripts.Ui;

namespace NeuralZeroProtocol.Scripts.Cards;

public partial class CardSelectionController
{
    public void GetUiInput(UiSelection uiSelection)
    {
        if (_currentState == CardSelectionState.SwapTheCards) return;
        
        if (_isInActionMenu) return;

        if (uiSelection is UiSelection.Left or UiSelection.Right)
        {
            if (_currentState != CardSelectionState.KeyboardMode) ActivateKeyboardMode();
            
            if (uiSelection == UiSelection.Left) KeyboardMoveLeft();
            
            if (uiSelection == UiSelection.Right) KeyboardMoveRight();
        } 
        else if (uiSelection is UiSelection.Down or UiSelection.Cancel)
        {
            KeyboardModeCancelled?.Invoke(false);
            
            _isInActionMenu = true;
            DeactivateKeyboardMode();
        }
        else if (uiSelection is UiSelection.Confirm)
        {
            if (_currentState != CardSelectionState.KeyboardMode) return;
            ConfirmCard();
        }
    }

    public void MoveToCardSystem()
    {
        if (_currentState == CardSelectionState.KeyboardMode) return;
        
        _isInActionMenu = false;
        ActivateKeyboardMode();
    }
    
    private void KeyboardMoveLeft()
    {
        if (_cardHand == null || _cardHand.Count == 0) return;

        int centerIndex = _cardHand.IndexOf(_keyboardHoveredCard);
        int toLeftCardIndex = centerIndex - 1;
        if (toLeftCardIndex < 0) toLeftCardIndex = _cardHand.Count - 1;

        _keyboardHoveredCard = _cardHand[toLeftCardIndex];
        KeyboardHoveredCardChanged?.Invoke(_keyboardHoveredCard);
    }

    private void KeyboardMoveRight()
    {
        if (_cardHand == null || _cardHand.Count == 0) return;
        
        int centerIndex = _cardHand.IndexOf(_keyboardHoveredCard);
        int toRightCardIndex = centerIndex + 1;
        if(toRightCardIndex >= _cardHand.Count) toRightCardIndex = 0;

        _keyboardHoveredCard = _cardHand[toRightCardIndex];
        KeyboardHoveredCardChanged?.Invoke(_keyboardHoveredCard);
    }

    private void ConfirmCard()
    {
        if (_currentState != CardSelectionState.KeyboardMode) return;
        if (_cardHand == null || _cardHand.Count == 0) return;
        if (_keyboardHoveredCard == null) return; 
        if (_keyboardHoveredCard == _centerCard) return;
        
        Card cardToSwap = _keyboardHoveredCard;
    
        // Deactivate keyboard mode and notify the battle menu
        _isInActionMenu = true;
        KeyboardModeCancelled?.Invoke(false);
        DeactivateKeyboardMode();   
        
        SwapWithCenter(cardToSwap);
    }
    
    // Helper Functions
    private void ActivateKeyboardMode()
    {
        _keyboardHoveredCard = _centerCard;
        KeyboardHoveredCardChanged?.Invoke(_keyboardHoveredCard);
        ChangeState(CardSelectionState.KeyboardMode);
        KeyboardModeActivated?.Invoke();
        
        _activationMousePos = GetGlobalMousePosition();
        _ignoreMouseMotion = false;
    }
    
    private void DeactivateKeyboardMode()
    {
        if (_currentState != CardSelectionState.KeyboardMode) return;
        
        KeyboardModeDeactivated?.Invoke();
        
        ChangeState(CardSelectionState.Idle);
        _keyboardHoveredCard = null;
        _ignoreMouseMotion = false;
    }
}