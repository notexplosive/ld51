using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD51;

public class Crop
{
    public static Crop Potato = new();

    public void Draw(Painter painter, Vector2 renderPos, Depth depth)
    {
        Client.Assets.GetAsset<SpriteSheet>("Plants").DrawFrame(painter, 0, renderPos, Scale2D.One,
            new DrawSettings {Color = Color.White, Depth = depth, Origin = DrawOrigin.Center});
    }

    public void Update(float dt)
    {
        
    }
}
