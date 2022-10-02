using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class ErrorToast : BaseComponent
{
    private readonly TweenableFloat _opacity = new(0f);
    private string _text;
    private readonly SequenceTween _tween = new();

    public ErrorToast(Actor actor) : base(actor)
    {
    }

    public void ShowError(string errorText)
    {
        _tween.Clear();
        _opacity.Value = 0f;
        _text = errorText;
        _tween.Add(new Tween<float>(_opacity, 1f, 0.25f, Ease.CubicFastSlow));
        _tween.Add(new WaitSecondsTween(1));
        _tween.Add(new Tween<float>(_opacity, 0f, 0.15f, Ease.CubicSlowFast));
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        var rect = new Rectangle(Transform.Position.ToPoint(),
            new Point(Client.Window.RenderResolution.X, A.BigFont.FontSize));

        var bgRect = rect;
        bgRect.Inflate(0, 10);
        bgRect.Inflate(0, -10 * _opacity);
        painter.DrawRectangle(
            bgRect,
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(_opacity.Value / 2f), Depth = Transform.Depth});

        painter.DrawStringWithinRectangle(A.BigFont, _text, rect, Alignment.Center, new DrawSettings
            {
                Color = Color.OrangeRed.WithMultipliedOpacity(_opacity.Value), Depth = Transform.Depth - 1
            }
        );
    }
}
