using System;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.HitTesting;
using ExplogineMonoGame.Input;
using MachinaLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class MainMenu : BaseComponent
{
    private readonly Action _startGame;

    public MainMenu(Actor actor, Action startGame) : base(actor)
    {
        _startGame = startGame;
    }

    public override void Draw(Painter painter)
    {
        var wholeScreen = new Rectangle(Point.Zero, Client.Window.RenderResolution);
        var inset = wholeScreen;
        inset.Inflate(0, -200);

        var insetMore = wholeScreen;
        insetMore.Inflate(0, -320);
        
        painter.DrawStringWithinRectangle(A.TitleScreenFont, "Last Fall",
            inset, Alignment.TopCenter, new DrawSettings());
        
        painter.DrawStringWithinRectangle(A.CardTextFont, "by NotExplosive",
            insetMore, Alignment.TopCenter, new DrawSettings());
        
        painter.DrawStringWithinRectangle(A.BigFont, "Press F4 to toggle Fullscreen\nClick anywhere to begin",
            inset, Alignment.Center, new DrawSettings());
        
        painter.DrawStringWithinRectangle(A.BigFont, "notexplosive.net",
            inset, Alignment.BottomCenter, new DrawSettings());
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
