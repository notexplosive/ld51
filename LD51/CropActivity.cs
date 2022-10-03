using Microsoft.Xna.Framework;

namespace LD51;

public record CropActivity(string Description, CropEvent Behavior)
{
    public static CropActivity GainEnergy(int amount)
    {
        return new CropActivity($"Gain {amount} Energy", (data, _) => { Fx.GainEnergy(data.Position.Rectangle.Center.ToVector2(), amount); });
    }

    public static CropActivity GainCard(CropTemplate template)
    {
        return new CropActivity($"Put a {template.Name} in Discard Pile", (data, _) => { Fx.PutCardInDiscard(data.Position.Rectangle.Center.ToVector2(), template); });   
    }
    
    public static CropActivity GainCardOfRarity(Rarity rarity, string excludingByName = null)
    {
        return new CropActivity($"Put a random {rarity} Seed in Discard Pile", (data, _) =>
        {
            CropTemplate excludedCrop = null;
            if(!string.IsNullOrEmpty(excludingByName))
            {
                excludedCrop = CropTemplate.GetByName(excludingByName);
            }
            
            Fx.PutCardInDiscard(data.Position.Rectangle.Center.ToVector2(), CropTemplate.GetRandomOfRarity(rarity, excludedCrop));
        });   
    }

    public static CropActivity ForceAdjacentTilesToBeWatered()
    {
        return new CropActivity("Water neighboring Soil", (data, _) =>
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var position = data.Position.GridPosition + new Point(x, y);
                    data.Tiles.SetContentAt(position, TileContent.WateredL3);
                    Fx.WaterTile(position);
                }
                
            }
        });
    }
    
    public static CropActivity TillAdjacentCrops()
    {
        return new CropActivity("Till neighboring Dirt", (data, _) =>
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var position = data.Position.GridPosition + new Point(x, y);
                    var content = data.Tiles.GetContentAt(position);

                    if (!content.IsDead)
                    {
                        data.Tiles.SetContentAt(position, TileContent.Tilled);
                        // Fx.TillTile(position);
                    }
                }
                
            }
        });
    }

    public static CropActivity Recycle()
    {
        return new CropActivity("Recycle into Discard",
            (data, _) =>
        {
            Fx.PutCardInDiscard(data.Position.Rectangle.Center.ToVector2(), data.Template); 
        });
    }

    public static CropActivity RestoreAdjacentCrops()
    {
        return new CropActivity("Restore neighboring Unusable Soil into Dirt", (data, _) =>
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var position = data.Position.GridPosition + new Point(x, y);
                    var content = data.Tiles.GetContentAt(position);

                    if (content.IsDead)
                    {
                        data.Tiles.SetContentAt(position, TileContent.Dirt);
                        // Fx.RestoreTile(position);
                    }
                }
                
            }
        });
    }

    public static CropActivity DrawCard(int number)
    {
        var s = number > 0 ? "s" : ""; 
        return new CropActivity($"Draw {number} Seed{s} from the deck at no cost", (data, _) =>
        {
            for (int i = 0; i < number; i++)
            {
                LudumCartridge.Ui.Inventory.DrawNextCard(true);
            }
        });
    }

    public static CropActivity ReShuffleForFree()
    {
        return new CropActivity("Reshuffle Discard Pile into Deck at no cost", (data, _) => LudumCartridge.Ui.DiscardPile.Reshuffle());
    }

    public static CropActivity GrowAdjacentCrops()
    {
        return new CropActivity("Grow Neighboring Crops 1 Tick", (data, _) =>
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var position = data.Position.GridPosition + new Point(x, y);

                    if (LudumCartridge.World.Garden.HasCropAt(position))
                    {
                        LudumCartridge.World.Garden.GetCropAt(position).Grow(true); 
                    }
                }
                
            }
        });
    }

    public static CropActivity DoubleHarvestAdjacentCrops()
    {
        return new CropActivity("Harvest neighboring crops but trigger the On Harvest effect twice", (data, _) =>
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var position = data.Position.GridPosition + new Point(x, y);

                    if (LudumCartridge.World.Garden.HasCropAt(position))
                    {
                        var crop = LudumCartridge.World.Garden.GetCropAt(position);
                        if (crop.IsReadyToHarvest)
                        {
                            crop.Harvest(true);
                            crop.Harvest();
                        }
                    }
                }
                
            }
        });
    }

    public static CropActivity WinGame()
    {
        return new CropActivity("Summons another Golem", (data, _) =>
        {
            LudumCartridge.Cutscene.PlayEnding();
        });
    }

    public static CropActivity GrowSelf()
    {
        return new CropActivity("Grow one Tick", (data, _) =>
        {
            data.Garden.TryGetCropAt(data.Position.GridPosition).Grow();
        });
    }

    public static CropActivity DestroyOther()
    {
        return new CropActivity("Consume the crop", (_, other) =>
        {
            var crop = other.Garden.TryGetCropAt(other.Position.GridPosition);
            if (crop != null)
            {
                LudumCartridge.World.Garden.RemoveCropAt(other.Position);
            }
        });
    }
}