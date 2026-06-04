using System;
using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Resources.MoveData;


namespace NeuralZeroProtocol.Scripts.Cards
{
    public partial class CardHand : Node2D
    {
        public event Action<Card> CardAdded;

        private Path2D _path2D;

        public override void _Ready() 
        {
            _path2D = GetNode<Path2D>("../Path2D");
        }

        // private void SwapWithCenter(Card cardClicked);

        public Array<Card> CreateHandFromCurve(int cardCount, PackedScene cardScene, Array<MoveResource> moves)
		{
			Curve2D curve = _path2D.Curve;
            float totalLength = curve.GetBakedLength();

            Array<Card> cards = new();

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
                Vector2 globalPos = _path2D.ToGlobal(localOnCurve);
                
                Card card = cardScene.Instantiate<Card>();
                
                AddChild(card);
                card.ChangeCardSkin(moves[i]);
                card.Position = ToLocal(globalPos);
                
                float maxRotation = Mathf.DegToRad(10f);
                float normalized = indexOffset / ((cardCount - 1) / 2f);
                card.Rotation = normalized * maxRotation;

                cards.Add(card);
                CardAdded?.Invoke(card);
            }
            
            // Assign Z indexes: Example. 3, 4, 5, 4, 3
            int centerIndex = cardCount / 2;
            int maxZ = 5;
            for (int i = 0; i < cards.Count; i++)
            {
                int distanceFromCenter = Math.Abs(i - centerIndex);
                int z = maxZ - distanceFromCenter;
                cards[i].ZIndex = z;
                cards[i].UpdatePriority();
            }

            return cards;
        }
    }
}

