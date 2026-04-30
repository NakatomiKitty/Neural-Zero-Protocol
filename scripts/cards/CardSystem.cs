using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Godot;
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

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated) WireNodes();
        }

        public override void _Ready()
        {
            CardSelectionController.SelectionChanged += OnSelectionChanged;
            CardHand.CardAdded += OnCardAdded;

            // always create 5 cards to the hand. 
            // TODO: ADD A SYSTEM IN THE FUTURE WHERE YOU CAN INCREASE YOUR HAND SIZE
        }

        public void UpdateCardSkin(MoveResource move)
        {
            CardHand.CreateHandFromPath(5, _card, move);
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