using System.Collections.Generic;
using ExplogineMonoGame.Input;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class DiscardPile : BaseComponent
{
    private readonly TextInBox _text;
    private Stack<CropTemplate> _content = new();
    private readonly Deck _deck;
    private readonly Box _box;

    public Rectangle Rectangle => _box.Rectangle;
    public int Count => _content.Count;

    public DiscardPile(Actor actor, Deck deck) : base(actor)
    {
        _box = RequireComponent<Box>();
        _text = RequireComponent<TextInBox>();
        _deck = deck;
    }

    public override void Update(float dt)
    {
        _text.Text = _content.Count.ToString();
    }

    public override void OnKey(Keys key, ButtonState state, ModifierKeys modifiers)
    {
        if (key == Keys.Q && modifiers.ControlShift && state == ButtonState.Pressed && !LudumCartridge.Cutscene.IsPlaying())
        {
            LudumCartridge.Cutscene.GoToShop();
        }
    }

    public void Reshuffle()
    {
        var deck = LudumCartridge.Ui.Deck;
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
