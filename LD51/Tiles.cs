﻿using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.HitTesting;
using ExplogineMonoGame.Input;
using MachinaLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class Tiles : BaseComponent
{
    public delegate void TileEvent(TilePosition tilePosition);

    private readonly Dictionary<Point, TileContent> _content = new();

    public Tiles(Actor actor, Point dimensions) : base(actor)
    {
        Dimensions = dimensions;
        foreach (var tilePosition in AllTilesPositions())
        {
            _content[tilePosition.GridPosition] = ChooseRandomSoilContent();
        }
    }

    private TileContent ChooseRandomSoilContent()
    {
        var roll = Client.Random.Clean.NextFloat();

        if (roll < 0.15)
        {
            return TileContent.Dirt;
        }

        return TileContent.Dead;
    }

    public Point Dimensions { get; }

    public TilePosition? HoveredTile { get; set; }

    public override void OnMouseUpdate(Vector2 currentPosition, Vector2 worldDelta, Vector2 rawDelta,
        HitTestStack hitTestStack)
    {
        HoveredTile = null;
        foreach (var tile in AllTilesPositions())
        {
            hitTestStack.Add(tile.Rectangle, Transform.Depth, () => { OnTileHovered(tile); });
        }
    }

    public override void OnMouseButton(MouseButton button, Vector2 currentPosition, ButtonState state,
        HitTestStack hitTestStack)
    {
        if (button == MouseButton.Left && state == ButtonState.Pressed && !LudumCartridge.Cutscene.IsPlaying())
        {
            if (HoveredTile.HasValue)
            {
                OnTileTapped(HoveredTile.Value);
            }
        }
    }

    public event TileEvent TileTapped;

    private void OnTileTapped(TilePosition tilePosition)
    {
        TileTapped?.Invoke(tilePosition);
    }

    private void OnTileHovered(TilePosition tilePosition)
    {
        HoveredTile = tilePosition;
        
        var tapAction = LudumCartridge.World.GetTapAction(tilePosition, LudumCartridge.Ui.Inventory.GrabbedCard);

        var cropDescription = "";
        var hasCrop = LudumCartridge.World.Garden.HasCropAt(tilePosition);
        if (hasCrop)
        {
            var crop = LudumCartridge.World.Garden.GetCropAt(tilePosition);
            if (!crop.IsReadyToHarvest)
            {
                cropDescription =
                    $"{crop.Level + 1} / {crop.Template.EffectiveMaxLevel + 1}";
            }
        }

        var newline = "\n";

        if (cropDescription == "")
        {
            newline = "";
        }
        
        if (tapAction is TapError && hasCrop)
        {
            var crop = LudumCartridge.World.Garden.GetCropAt(tilePosition);
            LudumCartridge.Ui.Tooltip.Set(crop.Template.Name, $"{cropDescription}{newline}{GetContentAt(tilePosition).Name}");
        }
        else
        {
            LudumCartridge.Ui.Tooltip.Set(tapAction.Title, $"{tapAction.Description}{newline}{cropDescription}");
        }

    }

    public void SetContentAt(TilePosition tilePosition, TileContent content)
    {
        _content[tilePosition.GridPosition] = content;
    }

    public void SetContentAt(Point gridPosition, TileContent content)
    {
        _content[gridPosition] = content;
    }

    public IEnumerable<TilePosition> AllTilesPositions()
    {
        for (var y = 0; y < Dimensions.Y; y++)
        {
            for (var x = 0; x < Dimensions.X; x++)
            {
                var xy = new Point(x, y);
                yield return new TilePosition(
                    xy,
                    GridPosToRectangle(xy));
            }
        }
    }

    private Rectangle GridPosToRectangle(Point xy)
    {
        return new Rectangle(new Point(xy.X * A.TileRect.Width, xy.Y * A.TileRect.Height), A.TileRect.Size);
    }

    public TileContent GetContentAt(TilePosition tilePosition)
    {
        return _content[tilePosition.GridPosition];
    }

    public TileContent GetContentAt(Point gridPosition)
    {
        return _content[gridPosition];
    }

    public Rectangle GetRectangleAt(Point gridPosition)
    {
        return GridPosToRectangle(gridPosition);
    }
}
