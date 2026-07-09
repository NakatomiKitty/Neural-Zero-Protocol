using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using NeuralZeroProtocol.Scripts.Resources.MoveData;

namespace NeuralZeroProtocol.Scripts.Cards;

// TODO: ADD A DISCARD POOL AND MAKE IT SO WHEN MOVES ARE ABOUT TO BE LESS THAN 5, IT TAKES FROM THE DISCARD POOL
public readonly record struct CardLayoutData(Vector2 LocalPosition, float Rotation, int ZIndex, MoveResource Move);

public partial class CardHand : Node2D
{
    public event Action<Card> CardAdded;
    private Path2D _path2D;

    public void Initialize(Path2D path2D) => _path2D = path2D;

    public List<CardLayoutData> CalculateCardLayout(Curve2D curve, Array<MoveResource> moves)
    {
        int moveCount = Mathf.Min(moves.Count, 5);
        
        float totalLength = curve.GetBakedLength();
        
        float spread = 0.7f;
        float totalSpan = totalLength * spread;
        float spacing = (moveCount > 1) ? totalSpan / (moveCount - 1) : 0;
        float centerOffset = totalLength / 2f;
        

        List<CardLayoutData> layoutData = new();
        
        int centerIndex = moveCount / 2;
        int maxZ = 5;

        for (int i = 0; i < moveCount; i++)
        {
            // Math for the positions of the cards
            float indexOffset = i - (moveCount - 1) / 2f;
            float offset = centerOffset + indexOffset * spacing;
            Vector2 localOnCurve = curve.SampleBaked(offset);
            
            // Math for the rotations of the cards
            float maxRotation = Mathf.DegToRad(10f);
            float normalized = indexOffset / ((moveCount - 1) / 2f);
            float rotation = normalized * maxRotation;
            
            // Math for the Z-Indices of the cards. Should be 3, 4, 5, 4, 3
            int distanceFromCenter = Math.Abs(i - centerIndex);
            int z = maxZ - distanceFromCenter;
            
            layoutData.Add(new CardLayoutData(localOnCurve, rotation, z, moves[i]));
        }

        return layoutData;
    }
    
    public Array<Card> CreateHandFromCurve(PackedScene cardScene, Array<MoveResource> moves)
    {
        List<CardLayoutData> layout = CalculateCardLayout(_path2D.Curve, moves);
        
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


