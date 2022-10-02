﻿using System.Collections.Generic;
using ExplogineMonoGame.HitTesting;
using ExplogineMonoGame.Input;
using MachinaLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class Tiles : BaseComponent
{
    public delegate void TileEvent(TilePosition tilePosition);

    private readonly Dictionary<TilePosition, TileContent> _content = new();

    public Tiles(Actor actor, Point dimensions) : base(actor)
    {
        Dimensions = dimensions;
        foreach (var tilePosition in AllTilesPositions())
        {
            _content[tilePosition] = TileContent.Dirt;
        }
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
        if (button == MouseButton.Left && state == ButtonState.Pressed)
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
        var content = GetContentAt(tilePosition);

        var description = $"Costs {content.UpgradeCost()} Energy";
        var skip = false;

        if (LudumCartridge.World.Garden.HasCropAt(tilePosition))
        {
            var crop = LudumCartridge.World.Garden.GetCropAt(tilePosition);

            description +=
                $"\n{crop.Template.Name} {crop.Level + 1} / {crop.Template.EffectiveMaxLevel + 1}";

            if (crop.IsReadyToHarvest)
            {
                LudumCartridge.Ui.Tooltip.Set($"Harvest {crop.Template.Name}", "Harvest it!");
                skip = true;
            }
        }

        if (LudumCartridge.Ui.Inventory.HasGrabbedCard())
        {
            skip = true;

            if (content.IsWet)
            {
                var heldCrop = LudumCartridge.Ui.Inventory.GrabbedCard.CropTemplate.Name;
                LudumCartridge.Ui.Tooltip.Set($"Plant {heldCrop}",
                    $"Plant {heldCrop} in {content.Name}");
            }
            else
            {
                LudumCartridge.Ui.Tooltip.Set($"Can't Plant There",
                    "Need Wet Soil");
            }
        }

        if (!skip)
        {
            LudumCartridge.Ui.Tooltip.Set($"{content.UpgradeVerb} {content.Name}", description);
        }
    }

    public void PutTileContentAt(TilePosition tilePosition, TileContent content)
    {
        _content[tilePosition] = content;
    }

    public IEnumerable<TilePosition> AllTilesPositions()
    {
        for (var y = 0; y < Dimensions.Y; y++)
        {
            for (var x = 0; x < Dimensions.X; x++)
            {
                yield return new TilePosition(
                    new Point(x, y),
                    new Rectangle(new Point(x * A.TileRect.Width, y * A.TileRect.Height), A.TileRect.Size));
            }
        }
    }

    public TileContent GetContentAt(TilePosition tilePosition)
    {
        return _content[tilePosition];
    }
}
