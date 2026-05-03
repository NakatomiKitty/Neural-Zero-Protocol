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
        public const float TWEEN_DURATION = 0.03f;
        public const int BASE_Z = 5;
        public const int HOVER_Z = 9;
        public const int SELECTED_Z = 10;
        
        private static readonly PackedScene _card = GD.Load<PackedScene>("res://scenes/card.tscn");

        [Node] public CardHand CardHand;
        [Node] public CardHoverController CardHoverController;
        [Node] public CardSelectionController CardSelectionController;

        public Dictionary<Card, int> OriginalZIndexes = new Dictionary<Card, int>();

        public Dictionary<Card, Vector2> CardBasePositions = new Dictionary<Card, Vector2>();


        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated) WireNodes();
        }

        public override void _Ready()
        {
            CardSelectionController.SelectionChanged += OnSelectionChanged;
            CardSelectionController.GetHighlightedCard += OnGetHighlightedCard;
            CardSelectionController.SwappingStateChanged += CardHoverController.OnSwappingStateChanged;
            CardHand.CardAdded += OnCardAdded;
            CardHand.GetCenterCard += OnGetCenterCard;
            
        }

        public async Task CreateHandFromMoves(Array<MoveResource> moves)
        {
            // First, clears any remaining cards 
            await ClearCardRegistry();
            // always create 5 cards to the hand. 
            // TODO: ADD A SYSTEM IN THE FUTURE WHERE YOU CAN INCREASE YOUR HAND SIZE
            // Create the deck while passing down the array
            CardHand.CreateHandFromCurve(5, _card, moves);

            CardSelectionController.SetCardHand(CardHand.GetChildren());
        }

        public void GetUIInput(UISelection uiSelection)
        {
            if (uiSelection == UISelection.None) return;

            if (CardSelectionController.IsSwapping) return;

            if (CardSelectionController.MouseModeActive) 
                CardSelectionController.MouseModeActive = false;

            switch (uiSelection)
            {
                case UISelection.Left:
                    CardSelectionController.MoveLeft();
                    break;
                case UISelection.Right:
                    CardSelectionController.MoveRight();
                    break;
                case UISelection.Confirm:
                    CardSelectionController.ConfirmCard();
                    break;
            }
        }

        private void OnCardAdded(Card card) => ConnectCard(card);

        private void OnGetCenterCard(Card centerCard) => CardSelectionController.SetCenterCard(centerCard);

        private void OnSelectionChanged(Card oldCard, Card newCard)
        {
            CardHoverController.OnCardDeselected(oldCard);

            if (newCard != null) CardHoverController.OnCardSelected(newCard);
        }

        private void OnGetHighlightedCard(Card card)
        {
            CardHoverController.ForceHighlight(card);
        }

        private void ConnectCard(Card card)
		{
			card.Hovered += CardHoverController.OnHoveredOverCard;
    		card.NotHovered += CardHoverController.OnHoveredOffCard;
            card.Clicked += CardSelectionController.OnCardClicked;
		}

        private async Task ClearCardRegistry()
        {
            var children = CardHand.GetChildren().ToArray();
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