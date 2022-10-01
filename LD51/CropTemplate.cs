namespace LD51;

public record CropTemplate(int TickLength, CropLevel CropLevel, CropTriggers CropTriggers)
{
    public static CropTemplate Potato = new(10, new CropLevel(3, 0),
        new OnHarvestTrigger(data => { Fx.GainEnergy(data.Position.Rectangle.Center.ToVector2(), 10); }));

    public int EffectiveMaxLevel => CropLevel.MaxLevel - 1;

    public Crop CreateCrop(Garden garden, TilePosition position, Tiles tiles)
    {
        var crop = new Crop(garden, this, tiles, position);

        crop.Harvested += CropTriggers.OnHarvest;
        crop.Grew += CropTriggers.OnGrow;
        crop.FinishedGrowing += CropTriggers.OnFinishedGrowing;

        return crop;
    }
}
