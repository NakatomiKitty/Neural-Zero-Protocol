using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using Godot;
using GodotUtilities;

namespace NeuralZeroProtocol.Scripts.Cards
{
    [Scene]
    public partial class CardSystem : Node2D
    {
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
            CardHand.CreateHandFromPath(5, _card);
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