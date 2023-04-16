using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExTween;
using ExTweenMonoGame;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class CutsceneDeck : BaseComponent
{
    private readonly TweenableVector2 _deckPosition = new();
    private readonly IRuntime _runtime;
    private readonly Dictionary<int, ChildCard> _children = new();
    private float _totalTime;

    public CutsceneDeck(Actor actor, TweenableVector2 deckPosition, IRuntime runtime) : base(actor)
    {
        _deckPosition = deckPosition;
        _runtime = runtime;
    }

    public int NumberOfCards { get; set; }

    public override void Update(float dt)
    {
        _totalTime += dt / 5;
        Transform.Position = _deckPosition.Value;

        var toRemove = new List<int>();
        foreach (var kv in _children)
        {
            var child = kv.Value;
            var key = kv.Key;
            if (child.IsPulling)
            {
                child.Time -= dt * 5;
            }
            else
            {
                child.Time += dt * 5;
            }

            if (child.Time > 1)
            {
                child.Time = 1;
            }

            if (child.Time < 0)
            {
                toRemove.Add(key);
            }
        }

        foreach (var key in toRemove)
        {
            _children.Remove(key);
        }
    }

    public override void Draw(Painter painter)
    {
        var sheet = Client.Assets.GetAsset<NinepatchSheet>("Card-Back");

        void DrawCardAt(Vector2 position, Depth depth)
        {
            sheet.DrawFullNinepatch(painter,
                new Rectangle((position - A.CardSize.ToVector2() / 2).ToPoint(), A.CardSize),
                InnerOuter.Inner,
                Transform.Depth + depth);
        }

        if (_children.Count != NumberOfCards)
        {
            DrawCardAt(Transform.Position, new Depth(0));
        }

        int index = 0;
        foreach (var child in _children.Values)
        {
            DrawCardAt(Transform.Position + new Vector2(MathF.Cos(child.Angle + _totalTime), MathF.Sin(child.Angle + _totalTime)) * child.Radius,
                new Depth(-index));
            index++;
        }
    }

    public ITween ShootCard(int index)
    {
        return new CallbackTween(
            () =>
            {
                Client.SoundPlayer.Play("draw-card", new SoundEffectSettings());
                _children.Add(index, new ChildCard(_runtime, (float) index / NumberOfCards * MathF.PI * 2));
            });
    }

    public ITween PullCardIn(int index)
    {
        return new CallbackTween(() =>
        {
            _children[index].Pull();
            Client.SoundPlayer.Play("paper", new SoundEffectSettings());
        });
    }

    public record ChildCard(IRuntime Runtime, float Angle)
    {
        public bool IsPulling;
        public float Time;

        public float Radius => Ease.CubicFastSlow(Time) * Runtime.Window.RenderResolution.Y / 3f;

        public void Pull()
        {
            IsPulling = true;
        }
    }
}
