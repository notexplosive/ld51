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
    public string Text { get; set; }
    public Color Color { get; set; }

    public TextInBox(Actor actor, Font font, string text) : base(actor)
    {
        _box = RequireComponent<Box>();
        _font = font;
        Text = text;
        Color = Color.Black;
    }

    public override void Draw(Painter painter)
    {
        painter.DrawStringWithinRectangle(_font, Text, _box.Rectangle, Alignment.Center,
            new DrawSettings {Color = Color, Depth = Transform.Depth - 1});
    }
}
