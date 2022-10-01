using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class SelectedTileRenderer : BaseComponent
{
    private readonly Tiles _tiles;

    public SelectedTileRenderer(Actor actor) : base(actor)
    {
        _tiles = RequireComponent<Tiles>();
    }
    
    public override void Draw(Painter painter)
    {
        if (_tiles.HoveredTile.HasValue)
        {
            var rect = _tiles.HoveredTile.Value.Rectangle;
            rect.Inflate(-10, -10);
            painter.DrawRectangle(rect,
                new DrawSettings {Color = Color.White.WithMultipliedOpacity(0.25f), Depth = Transform.Depth - 1});
        }
    }
}
