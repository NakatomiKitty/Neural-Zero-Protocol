using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Cards
{
    /// <summary>
    /// Main hub for everything card related
    /// I really need to name things better
    /// </summary>
    
    [Scene]
    public partial class CardSystem : Node2D
    {
        
        
        private static readonly PackedScene Card = GD.Load<PackedScene>("res://scenes/card.tscn");
        
        public const int HoverZ = 11;
        public const int SelectedZ = 10;
        
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
            CardSelectionController.GetKeyboardHoveredCard += OnKeyboardHoveredCardReceived;
            CardSelectionController.SwappingStateChanged += CardHoverController.OnSwappingStateChanged;
            CardSelectionController.KeyboardModeDeactivated += OnKeyboardModeDeactivated;
            
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

        private void ConnectCardSignals(Card card) => ConnectCard(card);
        
        private void OnKeyboardHoveredCardReceived(Card card) => CardHoverController.ForceHighlight(card);
        
        private void OnKeyboardModeDeactivated() => CardHoverController.ClearForcedHighlight();

        private void OnSelectionChanged(Card oldCard, Card newCard)
        {
            CardHoverController.OnCardDeselected();

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