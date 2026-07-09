using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Cards;

public readonly record struct CardLayoutData(Vector2 LocalPosition, float Rotation, int ZIndex, MoveResource Move);

public partial class CardHand : Node2D
{
    public event Action<Card> CardAdded;
    private Path2D _path2D;

    public void Initialize(Path2D path2D) => _path2D = path2D;

    public List<CardLayoutData> CalculateCardLayout(Curve2D curve, int cardCount, Array<MoveResource> moves)
    {
        float totalLength = curve.GetBakedLength();
        
        float spread = 0.7f;
        float totalSpan = totalLength * spread;
        float spacing = (cardCount > 1) ? totalSpan / (cardCount - 1) : 0;
        float centerOffset = totalLength / 2f;

        int moveCount = moves.Count;

        List<CardLayoutData> layoutData = new();
        
        int centerIndex = cardCount / 2;
        int maxZ = 5;

        for (int i = 0; i < moveCount; i++)
        {
            // Math for the positions of the cards
            float indexOffset = i - (cardCount - 1) / 2f;
            float offset = centerOffset + indexOffset * spacing;
            offset = Mathf.Clamp(offset, 0, totalLength);
            Vector2 localOnCurve = curve.SampleBaked(offset);
            
            // Math for the rotations of the cards
            float maxRotation = Mathf.DegToRad(10f);
            float normalized = indexOffset / ((cardCount - 1) / 2f);
            float rotation = normalized * maxRotation;
            
            // Math for the Z-Indices of the cards. Should be 3, 4, 5, 4, 3
            int distanceFromCenter = Math.Abs(i - centerIndex);
            int z = maxZ - distanceFromCenter;
            
            layoutData.Add(new CardLayoutData(localOnCurve, rotation, z, moves[i]));
        }

        return layoutData;
    }
    
    public Array<Card> CreateHandFromCurve(int cardCount, PackedScene cardScene, Array<MoveResource> moves)
    {
        List<CardLayoutData> layout = CalculateCardLayout(_path2D.Curve, cardCount, moves);
        
        Array<Card> cards = new();

        foreach (CardLayoutData data in layout)
        {
            Vector2 globalPos = _path2D.ToGlobal(data.LocalPosition);
            
            Card card = cardScene.Instantiate<Card>();
            AddChild(card);
            
            card.ChangeCardSkin(data.Move);
            card.Position = ToLocal(globalPos);
            
            card.Rotation = data.Rotation;
            
            card.ZIndex = data.ZIndex;
            card.UpdatePriority();
            
            cards.Add(card);
            CardAdded?.Invoke(card);
        }
        
        return cards;
    }
}


