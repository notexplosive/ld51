namespace LD51;

public record CropTemplate(int TickLength, CropLevel CropLevel, CropEvents CropEvents)
{
    public static CropTemplate Potato = new(10, new CropLevel(3, 0),
        new OnHarvestEvent((position, garden) => { Fx.GainEnergy(position.Rectangle.Center.ToVector2(), 10); }));

    public int EffectiveMaxLevel => CropLevel.MaxLevel - 1;

    public Crop CreateCrop(Garden garden, TilePosition position, Tiles tiles)
    {
        var crop = new Crop(garden, this, tiles, position);

        crop.Harvested += CropEvents.OnHarvest;
        crop.Grew += CropEvents.OnGrow;
        crop.FinishedGrowing += CropEvents.OnFinishedGrowing;

        return crop;
    }
}
