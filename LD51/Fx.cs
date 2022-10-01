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
        var particle = Fx._uiScene.AddActor("EnergyParticle");
        particle.Transform.Position = Fx.GameSpaceToUiSpace(worldPosition);
        particle.Transform.Depth -= 100;
        new TextureRenderer(particle, Client.Assets.GetTexture("energy"));
        var tweenable = new TweenableVector2(() => particle.Transform.Position, v => particle.Transform.Position = v);
        var tweenOwner = new TweenOwner(particle);
        tweenOwner.Tween = new SequenceTween()
                .Add(new Tween<Vector2>(tweenable, Vector2.Zero, 1f, Ease.SineSlowFast))
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
