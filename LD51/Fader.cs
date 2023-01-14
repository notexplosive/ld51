using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class Fader : BaseComponent
{
    private readonly TweenableFloat _faderOpacity;
    private readonly IRuntime _runtime;

    public Fader(Actor actor, TweenableFloat faderOpacity, IRuntime runtime) : base(actor)
    {
        _faderOpacity = faderOpacity;
        _runtime = runtime;
    }

    public override void Draw(Painter painter)
    {
        painter.DrawRectangle(new Rectangle(Point.Zero, _runtime.Window.RenderResolution),
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(_faderOpacity.Value), Depth = Transform.Depth});
    }
}
