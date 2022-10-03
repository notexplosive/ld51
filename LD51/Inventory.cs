using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.HitTesting;
using ExplogineMonoGame.Input;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class Inventory : BaseComponent
{
    private readonly List<Card> _cardsInHand = new();

    public Inventory(Actor actor) : base(actor)
    {
    }

    public Card GrabbedCard { get; private set; }
    public int Count => _cardsInHand.Count;

    public override void OnMouseButton(MouseButton button, Vector2 currentPosition, ButtonState state,
        HitTestStack hitTestStack)
    {
        if (button == MouseButton.Right && state == ButtonState.Pressed && !LudumCartridge.Cutscene.IsPlaying())
        {
            ClearGrabbedCard();
        }
    }

    public override void OnKey(Keys key, ButtonState state, ModifierKeys modifiers)
    {
        if (state == ButtonState.Pressed && modifiers.None && !LudumCartridge.Cutscene.IsPlaying())
        {
            void TryGrab(int index)
            {
                var prevCard = GrabbedCard;
                ClearGrabbedCard();
                if (_cardsInHand.Count > index)
                {
                    if (prevCard != _cardsInHand[index])
                    {
                        Grab(_cardsInHand[index]);
                    }
                }
            }

            switch (key)
            {
                case Keys.D1:
                    TryGrab(0);
                    break;
                case Keys.D2:
                    TryGrab(1);
                    break;
                case Keys.D3:
                    TryGrab(2);
                    break;
                case Keys.D4:
                    TryGrab(3);
                    break;
                case Keys.D5:
                    TryGrab(4);
                    break;
                case Keys.D6:
                    TryGrab(5);
                    break;
                case Keys.Tab:
                    DrawNextCard();
                    break;
            }
        }
    }

    public void DrawNextCard(bool skipCost = false)
    {
        
        var cost = A.DrawCardCost;
        var canAfford = PlayerStats.Energy.CanAfford(cost) || skipCost;

        if (!IsFull() && LudumCartridge.Ui.Deck.IsNotEmpty() && canAfford)
        {
            Client.SoundPlayer.Play("draw-card", new SoundEffectOptions());
            if (!skipCost)
            {
                PlayerStats.Energy.Consume(cost);
            }

            AddCard(LudumCartridge.Ui.Deck.NextTemplate());
        }
    }

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
        return GrabbedCard == card;
    }

    public void Grab(Card card)
    {
        Client.SoundPlayer.Play("paper", new SoundEffectOptions());
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

    public bool IsEmpty()
    {
        return _cardsInHand.Count == 0;
    }

    public Card GetCard(int i)
    {
        return _cardsInHand[i];
    }
}
