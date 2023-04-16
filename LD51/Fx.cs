using System;
using System.Diagnostics;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51;

public static class Fx
{
    public static void GainEnergy(Vector2 worldPosition, int amount)
    {
        Fx.GainEnergyToast(worldPosition, amount);
        PlayerStats.Energy.Amount += amount;

        for (var i = 0; i < amount; i+= 5)
        {
            Fx.GainEnergyParticle(worldPosition);
        }
    }

    private static void GainEnergyToast(Vector2 worldPosition, int amount)
    {
        var particle = LudumCartridge.Ui.Scene.AddActor("EnergyParticle");
        particle.Transform.Position = Fx.GameSpaceToUiSpace(worldPosition);
        particle.Transform.Depth -= 100;

        new TextToast(particle, amount);
    }

    private static void GainEnergyParticle(Vector2 worldPosition)
    {
        var particleParent = LudumCartridge.Ui.Scene.AddActor("EnergyParticle");
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
            ;
    }

    public static Vector2 UiSpaceToGameSpace(Vector2 uiPosition)
    {
        return LudumCartridge.World.Scene.Camera.ScreenToWorld(
            LudumCartridge.Ui.Scene.Camera.WorldToScreen(uiPosition));
    }

    public static Vector2 GameSpaceToUiSpace(Vector2 gamePosition)
    {
        return LudumCartridge.Ui.Scene.Camera.ScreenToWorld(
            LudumCartridge.World.Scene.Camera.WorldToScreen(gamePosition));
    }

    public static readonly SequenceTween EventTween = new();

    public static void WaterTile(Point gridPosition)
    {
        var tiles = LudumCartridge.World.Tiles;
        if (tiles.GetContentAt(gridPosition).IsTilled)
        {
            
            Client.SoundPlayer.Play("splash", new SoundEffectSettings());

            var water = LudumCartridge.World.Scene.AddActor("Water");
            water.Transform.Position = tiles.GetRectangleAt(gridPosition).Center.ToVector2();
            water.Transform.Depth += 50;

            var spr = new SpriteRenderer(water,Client.Assets.GetAsset<SpriteSheet>("Water"));
            spr.Color = Color.White.WithMultipliedOpacity(0.5f);
            var tweenOwner = new TweenOwner(water);

            tweenOwner.Tween = new SequenceTween()
                .Add(new WaitSecondsTween(0.5f))
                .Add(new CallbackTween(() =>
                {
                    tiles.SetContentAt(gridPosition, TileContent.WateredL3);
                }))
                .Add(new CallbackTween(water.Destroy))
                ;
        }
    }
    
    public static void PutCardInDiscard(Vector2 worldPosition, CropTemplate template)
    {
        void Callback()
        {
            var particle = LudumCartridge.Ui.Scene.AddActor("CardParticle");
            particle.Transform.Position = Fx.GameSpaceToUiSpace(worldPosition);
            particle.Transform.Depth -= 100;
            var destination = LudumCartridge.Ui.DiscardPile.Rectangle.Center.ToVector2();

            var positionTweenable =
                new TweenableVector2(() => particle.Transform.Position, v => particle.Transform.Position = v);

            var travelVector = destination - particle.Transform.Position;
            travelVector.Normalize();

            var renderer = new DummyCardRenderer(particle, template);

            var scaleTweenable = new TweenableFloat(() => renderer.Scale, v => renderer.Scale = v);
            scaleTweenable.Value = 0.25f;

            var initialPhaseDuration = 0.15f;
            var duration = 1f;
            var tweenOwner = new TweenOwner(particle);
            tweenOwner.Tween = new SequenceTween()
                    .Add(new CallbackTween(()=>Client.SoundPlayer.Play("draw-card", new SoundEffectSettings())))
                    .Add(
                        new MultiplexTween()
                            .AddChannel(new Tween<Vector2>(positionTweenable,
                                particle.Transform.Position - new Vector2(0, 150), initialPhaseDuration,
                                Ease.SineFastSlow))
                    )
                    .Add(new WaitSecondsTween(0.15f))
                    .Add(
                        new MultiplexTween()
                            .AddChannel(new Tween<Vector2>(positionTweenable, destination, duration, Ease.SineFastSlow))
                            .AddChannel(new Tween<float>(scaleTweenable, 1f, duration / 2f, Ease.Linear))
                    )
                    .Add(new WaitSecondsTween(0.25f))
                    .Add(new CallbackTween(particle.Destroy))
                    .Add(new CallbackTween(() => LudumCartridge.Ui.DiscardPile.Add(template)))
                ;
        }

        Fx.EventTween.Add(new CallbackTween(Callback));
        Fx.EventTween.Add(new WaitSecondsTween(0.5f));
    }
}