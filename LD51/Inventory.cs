using System.Collections.Generic;
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
        new BoxRenderer(cardActor);
        new Hoverable(cardActor);
        new TextInBox(cardActor, A.CardTextFont, template.Name);
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

    public void Discard(Card card, DiscardPile discardPile)
    {
        card.Actor.Destroy();
        _cardsInHand.Remove(card);
        discardPile.Add(card.CropTemplate);
        ArrangeCards();
    }
}
