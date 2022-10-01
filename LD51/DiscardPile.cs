﻿using System.Collections.Generic;
using MachinaLite;

namespace LD51;

public class DiscardPile : BaseComponent
{
    private readonly TextInBox _text;
    private Stack<CropTemplate> _content = new();
    private readonly Deck _deck;

    public DiscardPile(Actor actor, Deck deck) : base(actor)
    {
        _text = RequireComponent<TextInBox>();
        _deck = deck;
    }

    public override void Update(float dt)
    {
        _text.Text = _content.Count.ToString();
    }

    public void Reshuffle(Deck deck)
    {
        while (_content.Count > 0)
        {
            deck.AddCard(_content.Pop());
        }

        deck.Shuffle();
    }

    public void Add(CropTemplate template)
    {
        _content.Push(template);
    }
}