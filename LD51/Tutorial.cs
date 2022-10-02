using System;
using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class Tutorial : BaseComponent
{
    private readonly ITween _click;
    private readonly TweenableFloat _val = new();

    private ITween _currentAnimation;

    private string _currentMessage;
    private float _time;
    private Queue<Trigger> _triggerQueue = new();

    public Tutorial(Actor actor) : base(actor)
    {
        _click = new SequenceTween()
                .Add(new CallbackTween(() => { _val.Value = 0; }))
                .Add(new Tween<float>(_val, 5, 0.25f, Ease.CubicFastSlow))
                .Add(new Tween<float>(_val, -5, 0.15f, Ease.CubicSlowFast))
                .Add(new Tween<float>(_val, 0, 0.05f, Ease.CubicSlowFast))
                .Add(new WaitSecondsTween(1f))
            ;
    }

    public Rectangle? HighlightedRect { get; private set; }

    public override void Update(float dt)
    {
        _time += dt;

        if (_triggerQueue.TryPeek(out Trigger result))
        {
            if (result.Condition())
            {
                result.Result();
                _triggerQueue.Dequeue();
            }
        }
    }

    public override void Draw(Painter painter)
    {
        if (HighlightedRect.HasValue)
        {
            foreach (var rect in Lines())
            {
                painter.DrawRectangle(rect, new DrawSettings {Color = Color.LimeGreen.WithMultipliedOpacity(0.75f)});
            }

            var textRect = HighlightedRect.Value;
            textRect.Inflate(10000, 0);
            textRect.Height = A.CardTextFont.FontSize + 5;
            textRect.Location = new Point(textRect.Location.X, HighlightedRect.Value.Location.Y - 100);

            painter.DrawStringWithinRectangle(A.CardTextFont, _currentMessage, textRect, Alignment.Center,
                new DrawSettings {Color = Color.LimeGreen.WithMultipliedOpacity(0.75f)});
        }
    }

    private IEnumerable<Rectangle> Lines()
    {
        if (HighlightedRect.HasValue)
        {
            var animatedRect = HighlightedRect.Value;
            animatedRect.Inflate(AnimationValue(), AnimationValue());
            yield return new Rectangle(animatedRect.Left, animatedRect.Top, 1, animatedRect.Height);
            yield return new Rectangle(animatedRect.Left, animatedRect.Top, animatedRect.Width, 1);
            yield return new Rectangle(animatedRect.Left, animatedRect.Bottom, animatedRect.Width, 1);
            yield return new Rectangle(animatedRect.Right, animatedRect.Top, 1, animatedRect.Height);
        }
    }

    private float AnimationValue()
    {
        _currentAnimation.JumpTo(_time % _currentAnimation.TotalDuration.Get());
        return _val;
    }

    public void TellPlayerToClickOn(Rectangle rectangle, string message)
    {
        HighlightedRect = rectangle;
        _currentAnimation = _click;
        _currentMessage = message;
    }

    public void AddTrigger(Func<bool> condition, Action result)
    {
        _triggerQueue.Enqueue(new Trigger(condition, result));
    }

    private readonly record struct Trigger(Func<bool> Condition, Action Result);

    public void Clear()
    {
        HighlightedRect = null;
    }
}
