using System;
using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Resources.MoveData;


namespace NeuralZeroProtocol.Scripts.Cards
{
    public partial class CardHand : Node2D
    {
        [Signal] public delegate void CardAddedEventHandler(Card card);
        [Signal] public delegate void GetCenterCardEventHandler(Card card);

        private Path2D _path2d;
        private CardSystem _cardSystem;

        public override void _Ready() 
        {
            _path2d = GetNode<Path2D>("../Path2D");
            _cardSystem = GetNode<CardSystem>("..");
        }

        // private void SwapWithCenter(Card cardClicked);

        public void CreateHandFromCurve(int cardCount, PackedScene cardScene, Array<MoveResource> moves)
		{
			Curve2D curve = _path2d.Curve;
            float totalLength = curve.GetBakedLength();

            Array<Card> cards = new Array<Card>();

            float spread = 0.7f;
            float totalSpan = totalLength * spread;
            float spacing = (cardCount > 1) ? totalSpan / (cardCount - 1) : 0;

            float centerOffset = totalLength / 2f;

            for (int i = 0; i < moves.Count; i++)
            {
                float indexOffset = i - (cardCount - 1) / 2f;
                float offset = centerOffset + indexOffset * spacing;
                offset = Mathf.Clamp(offset, 0, totalLength);

                Vector2 localOnCurve = curve.SampleBaked(offset);
                Vector2 globalPos = _path2d.ToGlobal(localOnCurve);

                Card card = cardScene.Instantiate<Card>();
                card.ChangeCardSkin(moves[i]);
                AddChild(card);
                card.Position = ToLocal(globalPos);
                _cardSystem.CardBasePositions[card] = card.Position;

                float maxRotation = Mathf.DegToRad(15f);
                float normalized = indexOffset / ((cardCount - 1) / 2f);
                card.Rotation = normalized * maxRotation;

                cards.Add(card);
            }

            // Assign Z indexes: Example. 3, 4, 5, 4, 3
            int centerIndex = cardCount / 2;          
            int maxZ = 5;                             
            
            for (int i = 0; i < cards.Count; i++)
            {
                int distanceFromCenter = Math.Abs(i - centerIndex);
                cards[i].ZIndex = maxZ - distanceFromCenter;
                cards[i].UpdatePriority();
                _cardSystem.OriginalZIndexes[cards[i]] = cards[i].ZIndex;
            }

            SetCenterCard(cards[cards.Count / 2]);

            foreach (var card in cards)
            {
                EmitSignal(SignalName.CardAdded, card);
            }
		}

        private void SetCenterCard(Card centerCard) => EmitSignal(SignalName.GetCenterCard, centerCard);
    }
}

