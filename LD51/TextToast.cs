using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class TextToast : BaseComponent
{
    private readonly int _amount;
    private readonly TweenableFloat _height;
    private readonly SequenceTween _tween;

    public TextToast(Actor actor, int amount) : base(actor)
    {
        _amount = amount;
        var targetHeight = 74;
        _height = new TweenableFloat(0);
        _tween = new SequenceTween()
                .Add(new Tween<float>(_height, targetHeight, 0.5f, Ease.CubicFastSlow))
                .Add(new Tween<float>(_height, targetHeight - 5, 0.15f, Ease.CubicSlowFast))
                .Add(new CallbackTween(Actor.Destroy))
            ;
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        var font = A.BigFont;
        var amount = _amount.ToString();
        var measureString = font.MeasureString(amount);
        painter.DrawStringAtPosition(font, amount,
            (Transform.Position - measureString / 2f).ToPoint() + new Point(0, (int) -_height.Value), new DrawSettings
            {
                Color = Color.Goldenrod
            });
    }
}
