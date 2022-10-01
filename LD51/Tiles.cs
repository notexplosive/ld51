using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.HitTesting;
using ExplogineMonoGame.Input;
using MachinaLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class Tiles : BaseComponent
{
    public delegate void TileEvent(Tile tile);

    public Tiles(Actor actor, Point dimensions) : base(actor)
    {
        Dimensions = dimensions;
    }

    public Point Dimensions { get; set; }

    public Tile? HoveredTile { get; set; }

    public override void Draw(Painter painter)
    {
        if (HoveredTile.HasValue)
        {
            painter.DrawRectangle(HoveredTile.Value.Rectangle,
                new DrawSettings {Color = Color.White.WithMultipliedOpacity(0.25f), Depth = Transform.Depth - 1});
        }
    }

    public override void OnMouseUpdate(Vector2 currentPosition, Vector2 worldDelta, Vector2 rawDelta,
        HitTestStack hitTestStack)
    {
        HoveredTile = null;
        foreach (var tile in AllTiles())
        {
            hitTestStack.Add(tile.Rectangle, Transform.Depth, () => { OnTileHovered(tile); });
        }
    }

    public override void OnMouseButton(MouseButton button, Vector2 currentPosition, ButtonState state,
        HitTestStack hitTestStack)
    {
        if (button == MouseButton.Left && state == ButtonState.Pressed)
        {
            if (HoveredTile.HasValue)
            {
                OnTileTapped(HoveredTile.Value);
            }
        }
    }

    public event TileEvent TileTapped;

    private void OnTileTapped(Tile tile)
    {
        TileTapped?.Invoke(tile);
    }

    private void OnTileHovered(Tile tile)
    {
        HoveredTile = tile;
    }

    private IEnumerable<Tile> AllTiles()
    {
        for (var y = 0; y < Dimensions.Y; y++)
        {
            for (var x = 0; x < Dimensions.X; x++)
            {
                yield return new Tile(
                    new Point(x, y),
                    new Rectangle(new Point(x * A.TileRect.Width, y * A.TileRect.Height), A.TileRect.Size));
            }
        }
    }
}