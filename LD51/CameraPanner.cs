using ExplogineMonoGame;
using ExplogineMonoGame.HitTesting;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class CameraPanner : BaseComponent
{
    public CameraPanner(Actor actor) : base(actor)
    {
    }

    public override void OnMouseUpdate(Vector2 currentPosition, Vector2 worldDelta, Vector2 rawDelta,
        HitTestStack hitTestStack)
    {
        if (LudumCartridge.Cutscene.IsPlaying())
        {
            return;
        }

        var screenPosition = Actor.Scene.Camera.WorldToScreen(currentPosition);
        if (!new Rectangle(Point.Zero, Client.Window.RenderResolution).Contains(screenPosition))
        {
            return;
        }

        var padding = 64;
        var moveSpeed = 10;

        var bottomRight = Client.Window.RenderResolution.ToVector2() - new Vector2(padding);

        if (screenPosition.X < padding)
        {
            Actor.Scene.Camera.Position += new Vector2(-moveSpeed, 0);
        }

        if (screenPosition.X > bottomRight.X)
        {
            Actor.Scene.Camera.Position += new Vector2(moveSpeed, 0);
        }

        if (screenPosition.Y > bottomRight.Y)
        {
            Actor.Scene.Camera.Position += new Vector2(0, moveSpeed);
        }

        if (screenPosition.Y < padding)
        {
            Actor.Scene.Camera.Position += new Vector2(0, -moveSpeed);
        }

        var camPos = Actor.Scene.Camera.Position;
        var viewSize = Client.Window.RenderResolution;
        var bounds = LudumCartridge.World.Tiles.Bounds();
        if (camPos.X < bounds.X)
        {
            camPos.X = 0;
        }
        
        if (camPos.Y < bounds.Y)
        {
            camPos.Y = 0;
        }

        if (camPos.X + viewSize.X > bounds.Right)
        {
            camPos.X = bounds.Right - viewSize.X;
        }
        
        if (camPos.Y + viewSize.Y > bounds.Bottom)
        {
            camPos.Y = bounds.Bottom - viewSize.Y;
        }

        Actor.Scene.Camera.Position = camPos;
    }
}
