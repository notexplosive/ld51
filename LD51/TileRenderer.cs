using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class TileRenderer : BaseComponent
{
    private readonly Tiles _tiles;

    public TileRenderer(Actor actor) : base(actor)
    {
        _tiles = RequireComponent<Tiles>();
    }

    public override void Draw(Painter painter)
    {
        for (var y = 0; y < _tiles.Dimensions.Y; y++)
        {
            for (var x = 0; x < _tiles.Dimensions.X; x++)
            {
                A.TileSheet.DrawFrame(painter, 0,
                    new Vector2(x * A.TileRect.Width, y * A.TileRect.Height),
                    Scale2D.One,
                    new DrawSettings {Color = Color.White, Depth = Transform.Depth});
            }
        }
    }
}
