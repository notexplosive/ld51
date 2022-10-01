using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class TextInBox : BaseComponent
{
    private readonly Box _box;
    private readonly Font _font;
    private readonly string _text;

    public TextInBox(Actor actor, Font font, string text) : base(actor)
    {
        _box = RequireComponent<Box>();
        _font = font;
        _text = text;
    }

    public override void Draw(Painter painter)
    {
        painter.DrawStringWithinRectangle(_font, _text, _box.Rectangle, Alignment.Center,
            new DrawSettings {Color = Color.Black, Depth = Transform.Depth - 1});
    }
}
