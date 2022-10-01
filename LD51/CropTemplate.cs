namespace LD51;

public record CropTemplate(int TickLength, CropLevel CropLevel)
{
    public static CropTemplate Potato = new(10, new CropLevel(3, 0));
    public int EffectiveMaxLevel => CropLevel.MaxLevel - 1;

    public Crop CreateCrop(Garden garden, TilePosition position, Tiles tiles)
    {
        return new Crop(garden, this, tiles, position);
    }
}
