using Godot;
using Godot.Collections;
using GodotUtilities;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Cards
{
    /// <summary>
    /// Main hub for everything card related
    /// </summary>
    
    [Scene]
    public partial class CardSystem : Node2D
    {
        public const float TWEEN_DURATION = 0.05f;
        
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
            CardHand.CardAdded += OnCardAdded;
            CardHand.GetCenterCard += OnGetCenterCard;
        }

        public void UpdateCardSkin(Array<MoveResource> moves)
        {
            // First, clears any remaining cards 
            foreach (Card card in CardHand.GetChildren())
            {
                card.QueueFree();
            }

            // always create 5 cards to the hand. 
            // TODO: ADD A SYSTEM IN THE FUTURE WHERE YOU CAN INCREASE YOUR HAND SIZE
            // Create the deck while passing down the array
            CardHand.CreateHandFromPath(5, _card, moves);
        }


        private void OnGetCenterCard(Card centerCard)
        {
            CardSelectionController.SetCenterCard(centerCard);
            CardSelectionController.SelectCard(centerCard);
        } 

        private void OnCardAdded(Card card) => ConnectCard(card);

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
    }
}