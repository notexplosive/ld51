using Microsoft.Xna.Framework;

namespace LD51;

public record SurroundedByCrops(int TickLength) : ICropGrowCondition
{
    public bool CanGrow(CropEventData data)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                
                var position = data.Position.GridPosition + new Point(x, y);

                if (!LudumCartridge.World.Garden.HasCropAt(position))
                {
                    return false;
                }
            }
                
        }

        return true;
    }

    public string Description()
    {
        return $"Grows every {TickLength} seconds only when surrounded by Crops";
    }
}
