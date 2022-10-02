using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class Inventory : BaseComponent
{
    private readonly List<Card> _cardsInHand = new();

    public Inventory(Actor actor) : base(actor)
    {
    }

    public Card GrabbedCard { get; private set; }

    public void AddCard(CropTemplate template)
    {
        var cardActor = Transform.AddActorAsChild("Card");
        cardActor.Transform.LocalDepth -= 10;
        new Box(cardActor, A.CardSize);
        new Hoverable(cardActor);
        new CardRenderer(cardActor, template);
        // new BoxRenderer(cardActor);
        // new TextInBox(cardActor, A.CardTextFont, template.Name);
        var card = new Card(cardActor, this, template);

        _cardsInHand.Add(card);

        ArrangeCards();
    }

    private void ArrangeCards()
    {
        var index = 0;
        var padding = 15;
        var startingX = 32;
        foreach (var card in _cardsInHand)
        {
            card.Transform.LocalPosition =
                new Vector2(A.CardSize.X * index + padding * index + startingX, card.Transform.LocalPosition.Y);
            index++;
        }
    }

    public void ClearGrabbedCard()
    {
        GrabbedCard = null;
    }

    public bool IsGrabbed(Card card)
    {
        return GrabbedCard != card;
    }

    public void Grab(Card card)
    {
        GrabbedCard = card;
    }

    public bool HasGrabbedCard()
    {
        return GrabbedCard != null;
    }
    
    public void Remove(Card card)
    {
        card.Actor.Destroy();
        _cardsInHand.Remove(card);
        ArrangeCards();
    }

    public bool IsFull()
    {
        return _cardsInHand.Count >= 6;
    }
}

public class CardRenderer : BaseComponent
{
    private readonly Box _box;
    private readonly float _opacity;
    private readonly CropTemplate _template;

    public CardRenderer(Actor actor, CropTemplate template) : base(actor)
    {
        _box = RequireComponent<Box>();
        _template = template;
        _opacity = 1f;
    }

    public override void Draw(Painter painter)
    {
        CardRenderer.DrawCard(painter, _box.Rectangle, _template, _opacity, 1f, Transform.Depth);
    }

    public static void DrawCard(Painter painter, Rectangle rectangle, CropTemplate template, float opacity,
        float textScale, Depth depth)
    {
        var ninePatchSheet = Client.Assets.GetAsset<NinepatchSheet>("Card-Patch");
        ninePatchSheet.DrawFullNinepatch(painter, rectangle, InnerOuter.Inner, depth, opacity);

        painter.DrawStringWithinRectangle(A.CardTextFont.WithFontSize((int) (A.CardTextFont.FontSize * textScale)),
            template.Name, rectangle, Alignment.Center,
            new DrawSettings {Depth = depth - 1, Color = Color.Black.WithMultipliedOpacity(opacity)});
    }
}
