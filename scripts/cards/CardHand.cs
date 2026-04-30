using System;
using System.Collections.Generic;
using Godot;


namespace NeuralZeroProtocol.Scripts.Cards
{
    public partial class CardHand : Node2D
    {
        [Signal] public delegate void CardAddedEventHandler(Card card);

        private Path2D _path2d;
        private CardSystem _cardSystem;

        public override void _Ready() 
        {
            _path2d = GetNode<Path2D>("../Path2D");
            _cardSystem = GetNode<CardSystem>("..");
        }

        public void CreateHandFromPath(int cardCount, PackedScene cardScene)
		{
			Curve2D curve = _path2d.Curve;
			float totalLength = curve.GetBakedLength();

            List<Card> cards = new List<Card>();

            float spacing = 105f; 
            float centerOffset = totalLength / 2f;

			for (int i = 0; i < cardCount; i++)
			{
				float indexOffset = i - (cardCount - 1) / 2f;

                float offset = centerOffset + indexOffset * spacing;

                offset = Mathf.Clamp(offset, 0, totalLength);

				Vector2 position = curve.SampleBaked(offset);

				Card card = cardScene.Instantiate<Card>();
				AddChild(card);

				card.Position = position;

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

            foreach (var card in cards)
            {
                EmitSignal(SignalName.CardAdded, card);
            }
		}
    }
}

