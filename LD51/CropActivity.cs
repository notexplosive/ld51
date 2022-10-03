using Microsoft.Xna.Framework;

namespace LD51;

public record CropActivity(string Description, CropEvent Behavior)
{
    public static CropActivity GainEnergy(int amount)
    {
        return new CropActivity($"Gain {amount} Energy", data => { Fx.GainEnergy(data.Position.Rectangle.Center.ToVector2(), amount); });
    }

    public static CropActivity GainCard(CropTemplate template)
    {
        return new CropActivity($"Put a {template.Name} in Discard Pile", data => { Fx.PutCardInDiscard(data.Position.Rectangle.Center.ToVector2(), template); });   
    }
    
    public static CropActivity GainCardOfRarity(Rarity rarity)
    {
        return new CropActivity($"Put a random {rarity} Seed in Discard Pile", data => { Fx.PutCardInDiscard(data.Position.Rectangle.Center.ToVector2(), CropTemplate.GetRandomOfRarity(rarity)); });   
    }

    public static CropActivity WaterAdjacentCrops()
    {
        return new CropActivity("Water neighboring Soil", data =>
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var position = data.Position.GridPosition + new Point(x, y);
                    var content = data.Tiles.GetContentAt(position);

                    if (content.IsTilled)
                    {
                        Fx.WaterTile(position);
                    }
                }
                
            }
        });
    }
    
    public static CropActivity TillAdjacentCrops()
    {
        return new CropActivity("Till neighboring Dirt", data =>
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
        return new CropActivity("Put self in Discard",
            data =>
        {
            Fx.PutCardInDiscard(data.Position.Rectangle.Center.ToVector2(), data.Template); 
        });
    }

    public static CropActivity RestoreAdjacentCrops()
    {
        return new CropActivity("Restore neighboring Unusable Soil into Dirt", data =>
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
        return new CropActivity($"Draw {number} Seed{s} from the deck at no cost", data =>
        {
            for (int i = 0; i < number; i++)
            {
                LudumCartridge.Ui.Inventory.DrawNextCard(true);
            }
        });
    }

    public static CropActivity ReShuffleForFree()
    {
        return new CropActivity("Reshuffle Discard Pile into Deck at no cost", data => LudumCartridge.Ui.DiscardPile.Reshuffle());
    }

    public static CropActivity GrowAdjacentCrops()
    {
        return new CropActivity("Grow Neighboring Crops 1 Tick", data =>
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
        return new CropActivity("Harvest neighboring crops but trigger the On Harvest effect twice", data =>
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
}