using System.Collections.Generic;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

internal class SeedInventory : BaseComponent
{
    private readonly List<Card> _cardsInHand = new();

    public SeedInventory(Actor actor) : base(actor)
    {
    }

    private Card _grabbedCard;

    public void AddCard()
    {
        var cardActor = Transform.AddActorAsChild("Card");
        new Box(cardActor, A.CardSize);
        new BoxRenderer(cardActor);
        new Hoverable(cardActor);
        var card = new Card(cardActor, this, new Crop());

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
        _grabbedCard = null;
    }

    public bool IsGrabbed(Card card)
    {
        return _grabbedCard != card;
    }

    public void Grab(Card card)
    {
        _grabbedCard = card;
    }

    public bool HasGrabbedCard()
    {
        return _grabbedCard != null;
    }

    public Crop GrabbedSeed()
    {
        return _grabbedCard.Crop;
    }

    public void DiscardGrabbedCard()
    {
        _grabbedCard.Actor.Destroy();
        _cardsInHand.Remove(_grabbedCard);
        ArrangeCards();
    }
}
