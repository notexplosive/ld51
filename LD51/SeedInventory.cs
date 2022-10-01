using System.Collections.Generic;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class SeedInventory : BaseComponent
{
    private readonly List<Card> _cardsInHand = new();

    public SeedInventory(Actor actor) : base(actor)
    {
    }

    public Card GrabbedCard { get; private set; }

    public void AddCard()
    {
        var cardActor = Transform.AddActorAsChild("Card");
        new Box(cardActor, A.CardSize);
        new BoxRenderer(cardActor);
        new Hoverable(cardActor);
        var card = new Card(cardActor, this, CropTemplate.Potato);

        _cardsInHand.Add(card);

        ArrangeCards();
    }

    private void ArrangeCards()
    {
        var index = 0;
        var padding = 15;
        foreach (var card in _cardsInHand)
        {
            card.Transform.LocalPosition =
                new Vector2(A.CardSize.X * index + padding * index, card.Transform.LocalPosition.Y);
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

    public void Discard(Card card)
    {
        card.Actor.Destroy();
        _cardsInHand.Remove(card);
        ArrangeCards();
    }
}
