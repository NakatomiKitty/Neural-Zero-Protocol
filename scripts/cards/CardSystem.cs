using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Resources.MoveData;
using NeuralZeroProtocol.Scripts.Ui;

namespace NeuralZeroProtocol.Scripts.Cards
{
    /// <summary>
    /// Main hub for everything card related
    /// </summary>
    
    [Scene]
    public partial class CardSystem : Node2D
    {
        public const int HoverZ = 11;
        public const int SelectedZ = 10;
        
        private static readonly PackedScene Card = GD.Load<PackedScene>("res://scenes/card.tscn");

        [Node] public CardHand CardHand;
        [Node] public CardHoverController CardHoverController;
        [Node] public CardSelectionController CardSelectionController;

        public Dictionary<Card, int> OriginalZIndexes = new();

        public Dictionary<Card, Vector2> CardBasePositions = new();


        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated) WireNodes();
        }

        public override void _Ready()
        {
            float designWidth = 1152f; // the resolution you designed the hand for
            
            float scale = GetViewport().GetVisibleRect().Size.X / designWidth;
            
            Scale = new Vector2(scale, scale);
            
            CardSelectionController.SelectionChanged += OnSelectionChanged;
            CardSelectionController.GetHighlightedCard += OnHighlightedCardReceived;
            CardSelectionController.SwappingStateChanged += CardHoverController.OnSwappingStateChanged;
            
            CardHand.CardAdded += ConnectCardSignals;
        }

        public async Task CreateHandFromMoves(Array<MoveResource> moves)
        {
            // First, clears any remaining cards 
            await ClearCardRegistry();
            
            // TODO: ADD A SYSTEM IN THE FUTURE WHERE YOU CAN INCREASE YOUR HAND SIZE
            // Create the deck while passing down the array
            Array<Card> cards = CardHand.CreateHandFromCurve(5, Card, moves);
            
            foreach (Card card in cards)
            {
                CardBasePositions[card] = card.Position;   // store position after layout
                OriginalZIndexes[card] = card.ZIndex;      // store ZIndex after assignment
            }
            
            
            Card centerCard = cards[cards.Count / 2];
            CardSelectionController.SetCenterCard(centerCard);
            
            CardSelectionController.SetCardHand(CardHand.GetChildren());
        }

        public void GetUiInput(UiSelection uiSelection)
        {
            if (uiSelection == UiSelection.None) return;

            if (CardSelectionController.IsSwapping) return;

            if (CardSelectionController.MouseModeActive) 
                CardSelectionController.MouseModeActive = false;

            switch (uiSelection)
            {
                case UiSelection.Left:
                    CardSelectionController.KeyboardMoveLeft();
                    break;
                case UiSelection.Right:
                    CardSelectionController.KeyboardMoveRight();
                    break;
                case UiSelection.Confirm:
                    CardSelectionController.ConfirmCard();
                    break;
            }
        }

        private void ConnectCardSignals(Card card) => ConnectCard(card);
        
        private void OnHighlightedCardReceived(Card card) => CardHoverController.ForceHighlight(card);

        private void OnSelectionChanged(Card oldCard, Card newCard)
        {
            CardHoverController.OnCardDeselected(oldCard);

            if (newCard != null) CardHoverController.OnCardSelected(newCard);
        }

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
    }
}