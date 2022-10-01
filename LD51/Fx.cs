using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using MachinaLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51;

public static class Fx
{
    private static Scene _gameScene;
    private static Scene _uiScene;

    public static void Setup(Scene gameScene, Scene uiScene)
    {
        Fx._gameScene = gameScene;
        Fx._uiScene = uiScene;
    }

    public static void GainEnergy(Vector2 worldPosition, int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            Fx.GainEnergyParticle(worldPosition);
        }
    }

    private static void GainEnergyParticle(Vector2 worldPosition)
    {
        var particleParent = Fx._uiScene.AddActor("EnergyParticle");
        particleParent.Transform.Position = Fx.GameSpaceToUiSpace(worldPosition);
        particleParent.Transform.Depth -= 100;
        var destination = Vector2.Zero;

        var travelVector = destination - particleParent.Transform.Position;
        travelVector.Normalize();
        particleParent.Transform.Angle = MathF.Atan2(travelVector.Y, travelVector.X);

        var particle = particleParent.Transform.AddActorAsChild("ParticleGraphic");
        new TextureRenderer(particle, Client.Assets.GetTexture("energy"));
        // var tweenable = new TweenableVector2(() => particle.Transform.Position, v => particle.Transform.Position = v);

        var parentPosition = new TweenableVector2(
            () => particleParent.Transform.Position,
            val => particleParent.Transform.Position = val);
        var childPosition = new TweenableVector2(
            () => particle.Transform.LocalPosition,
            val => particle.Transform.LocalPosition = val);

        var totalDuration = Client.Random.Dirty.NextFloat() * 0.25f + 0.5f;
        var tweenOwner = new TweenOwner(particle);
        tweenOwner.Tween = new SequenceTween()
                .Add(
                    new MultiplexTween()
                        .AddChannel(new Tween<Vector2>(parentPosition, destination, totalDuration, Ease.SineSlowFast))
                        .AddChannel(
                            new SequenceTween()
                                .Add(new Tween<Vector2>(childPosition,
                                    new Vector2(0,
                                        100 * Client.Random.Dirty.NextSign() * Client.Random.Dirty.NextFloat()),
                                    totalDuration / 2, Ease.SineFastSlow))
                                .Add(new Tween<Vector2>(childPosition, Vector2.Zero, totalDuration / 2,
                                    Ease.SineSlowFast))
                        )
                )
                .Add(new CallbackTween(particle.Destroy))
                .Add(new CallbackTween(() => PlayerStats.Energy.Amount += 1))
            ;
    }

    public static Vector2 UiSpaceToGameSpace(Vector2 uiPosition)
    {
        return Fx._gameScene.Camera.ScreenToWorld(Fx._uiScene.Camera.WorldToScreen(uiPosition));
    }

    public static Vector2 GameSpaceToUiSpace(Vector2 gamePosition)
    {
        return Fx._uiScene.Camera.ScreenToWorld(Fx._gameScene.Camera.WorldToScreen(gamePosition));
    }
}

public class TextureRenderer : BaseComponent
{
    private readonly Texture2D _texture2d;

    public TextureRenderer(Actor actor, Texture2D texture2d) : base(actor)
    {
        _texture2d = texture2d;
    }

    public override void Draw(Painter painter)
    {
        painter.DrawAtPosition(_texture2d, Transform.Position, Scale2D.One,
            new DrawSettings
                {Angle = Transform.Angle, Color = Color.White, Depth = Transform.Depth, Origin = DrawOrigin.Center});
    }
}
