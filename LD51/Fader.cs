using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class Fader : BaseComponent
{
    private readonly TweenableFloat _faderOpacity;

    public Fader(Actor actor, TweenableFloat faderOpacity) : base(actor)
    {
        _faderOpacity = faderOpacity;
    }

    public override void Draw(Painter painter)
    {
        painter.DrawRectangle(new Rectangle(Point.Zero, Client.Window.RenderResolution),
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(_faderOpacity.Value), Depth = Transform.Depth});
    }
}
