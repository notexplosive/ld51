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

    public static CropActivity WaterAdjacentCrops()
    {
        return new CropActivity("Water neighboring Soil", data =>
        {
            // todo: FX for this
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    var position = data.Position.GridPosition + new Point(x, y);
                    var content = data.Tiles.GetContentAt(position);

                    if (content.IsTilled)
                    {
                        data.Tiles.SetContentAt(position, TileContent.WateredL3);
                    }
                }
                
            }
        });
    }

    public static CropActivity Recycle()
    {
        return new CropActivity("Recycle back into Discard",
            data =>
        {
            Fx.PutCardInDiscard(data.Position.Rectangle.Center.ToVector2(), data.Template); 
        });
    }
}