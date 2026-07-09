using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Cards;

[Scene]
public partial class CardSystem : Node2D
{
    private static readonly PackedScene Card = GD.Load<PackedScene>("res://scenes/card.tscn");
    public const int HoverZ = 11;
    public const int SelectedZ = 10;
    
    [Node] public CardHand CardHand;
    [Node] public CardHoverController CardHoverController;
    [Node] public CardSelectionController CardSelectionController;
    [Node] public CardBorderController CardBorderController;
    [Node] public Path2D Path2D;
    
    private Dictionary<Card, int> _originalZIndexes = new();
    private Dictionary<Card, Vector2> _cardBasePositions = new();
    private Card _centerCard;
    
    public override void _Notification(int what)
    {
        if (what == NotificationSceneInstantiated) WireNodes();
    }

    public override void _Ready()
    {
        CardSelectionController.SelectionChanged += OnSelectionChanged;
        CardSelectionController.KeyboardHoveredCardChanged += OnKeyboardHoveredCardReceived;
        CardSelectionController.CurrentSelectedCardChanged += OnCurrentSelectedCardChanged;
        CardSelectionController.SwappingStateChanged += CardHoverController.OnSwappingStateChanged;
        CardSelectionController.KeyboardModeDeactivated += OnKeyboardModeDeactivated;
        
        CardHoverController.Initialize(this);
        CardHoverController.EnterCardSelection += CardBorderController.OnEnterCardSelection;
        CardHoverController.ExitCardSelection += CardBorderController.OnExitCardSelection;
        CardHoverController.ApplyBorderEffect += CardBorderController.OnApplyBorderEffect;
        
        CardHand.Initialize(Path2D);
        CardHand.CardAdded += ConnectCardSignals;
    }

    public override void _ExitTree()
    {
        _ = ClearCardRegistry();

        CardSelectionController.SelectionChanged -= OnSelectionChanged;
        CardSelectionController.SwappingStateChanged -= CardHoverController.OnSwappingStateChanged;
        CardSelectionController.KeyboardHoveredCardChanged -= OnKeyboardHoveredCardReceived;
        CardSelectionController.KeyboardModeDeactivated -= OnKeyboardModeDeactivated;

        CardHoverController.EnterCardSelection -= CardBorderController.OnEnterCardSelection;
        CardHoverController.ExitCardSelection -= CardBorderController.OnExitCardSelection;
        CardHoverController.ApplyBorderEffect -= CardBorderController.OnApplyBorderEffect;
    }
    
    public async Task CreateHandFromMoves(Array<MoveResource> moves)
    {
        await ClearCardRegistry();

        Array<Card> cards = CardHand.CreateHandFromCurve(Card, moves);

        foreach (Card card in cards)
        {
            _cardBasePositions[card] = card.Position;
            _originalZIndexes[card] = card.ZIndex;
        }

        CardHoverController.OriginalZIndexes = _originalZIndexes;
        CardSelectionController.OriginalZIndexes = _originalZIndexes;
        CardSelectionController.CardBasePositions = _cardBasePositions;

        _centerCard = cards[cards.Count / 2];

        CardSelectionController.SetCenterCard(_centerCard);
        CardBorderController.SetCenterCard(_centerCard);
        CardSelectionController.SetCardHand(CardHand.GetChildren());
    }

    public void OnMoveToCardSystem()
    {
        CardHoverController.OnActionMenuClosed();
        CardSelectionController.MoveToCardSystem();
    }

    public void OnKeyboardModeDeactivated()
    {
        CardHoverController.OnActionMenuOpened();
        CardHoverController.ClearKeyboardHover();
    }

    public Card DuplicateCenterCard()
    {
        Vector2 clonedCenterGlobalPosition = _centerCard.GlobalPosition;
        Vector2 clonedCenterGlobalScale = _centerCard.GlobalScale;
        float clonedCenterRotation = _centerCard.GlobalRotation;

        Card clonedCenterCard = (Card)_centerCard.Duplicate();
        clonedCenterCard.GlobalPosition = clonedCenterGlobalPosition;
        clonedCenterCard.GlobalScale = clonedCenterGlobalScale;
        clonedCenterCard.GlobalRotation = clonedCenterRotation;

        _centerCard.QueueFree();
        return clonedCenterCard;
    }
    
    private void ConnectCardSignals(Card card) => ConnectCard(card);

    private void ConnectCard(Card card)
    {
        card.Hovered += CardHoverController.OnHoveredOverCard;
        card.NotHovered += CardHoverController.OnHoveredOffCard;
        card.Clicked += CardSelectionController.OnCardClicked;
    }

    private async Task ClearCardRegistry()
    {
        Node[] children = CardHand.GetChildren().ToArray();
        foreach (Card card in children)
        {
            card.Hovered -= CardHoverController.OnHoveredOverCard;
            card.NotHovered -= CardHoverController.OnHoveredOffCard;
            card.Clicked -= CardSelectionController.OnCardClicked;
            card.QueueFree();
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }
    
    private void OnCurrentSelectedCardChanged(Card centerCard) => _centerCard = centerCard;

    private void OnKeyboardHoveredCardReceived(Card card) => CardHoverController.KeyboardHover(card);

    private void OnSelectionChanged(Card oldCard, Card newCard)
    {
        if (oldCard != null)
            CardHoverController.OnCardDeselected();

        if (newCard != null)
            CardHoverController.OnCardSelected(newCard);
    }
}