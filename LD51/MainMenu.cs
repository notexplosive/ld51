using System;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using MachinaLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class MainMenu : BaseComponent
{
    private readonly Action _startGame;
    private readonly IRuntime _runtime;

    public MainMenu(Actor actor, Action startGame, IRuntime runtime) : base(actor)
    {
        _startGame = startGame;
        _runtime = runtime;
    }

    public override void Draw(Painter painter)
    {
        var wholeScreen = new Rectangle(Point.Zero, _runtime.Window.RenderResolution);
        var inset = wholeScreen;
        inset.Inflate(0, -100);

        var insetMore = wholeScreen;
        insetMore.Inflate(0, -220);

        var textDrawSettings = new DrawSettings{Depth = Transform.Depth, Color = Color.White};
        var backgroundSettings = new DrawSettings{Depth = Transform.Depth + 100, Color = Color.White.WithMultipliedOpacity(0.5f), SourceRectangle = wholeScreen};
        
        painter.DrawAsRectangle(Client.Assets.GetAsset<TextureAsset>("MenuBackground").Texture, wholeScreen, backgroundSettings);
        
        painter.DrawStringWithinRectangle(A.TitleScreenFont, "Harvest Golem",
            inset, Alignment.TopCenter, textDrawSettings);
        
        painter.DrawStringWithinRectangle(A.CardTextFont, "by NotExplosive\nmusic by Crashtroid",
            insetMore, Alignment.TopCenter, textDrawSettings);
        
        painter.DrawStringWithinRectangle(A.TooltipTitleFont, "Press F4 to toggle Fullscreen\nClick anywhere to begin",
            inset, Alignment.Center, textDrawSettings);
        
        painter.DrawStringWithinRectangle(A.TooltipTitleFont, "notexplosive.net -- crashtroid.carrd.co",
            inset, Alignment.BottomCenter, textDrawSettings);
    }

    public override void OnMouseButton(MouseButton button, Vector2 currentPosition, ButtonState state, HitTestStack hitTestStack)
    {
        if (state == ButtonState.Released)
        {
            _startGame();
            Actor.Destroy();
        }
    }
}
