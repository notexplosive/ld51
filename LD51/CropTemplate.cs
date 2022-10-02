namespace LD51;

public enum AlwaysNever
{
    Always,
    Never
}

public record CropTemplate(string Name, int TickLength, AlwaysNever RecycleStatus, CropLevel CropLevel, CropTriggers CropTriggers)
{
    public static CropTemplate Potato = new(
        "Potato", 
        10, 
        AlwaysNever.Always,
        new CropLevel(3, 0),
        new OnHarvestTrigger(
            data => { Fx.GainEnergy(data.Position.Rectangle.Center.ToVector2(), 15); })
        );
    
    public static CropTemplate Carrot = new(
        "Carrot", 
        10,
        AlwaysNever.Always,
        new CropLevel(2, 3),
        new OnHarvestTrigger(
            data => { Fx.PutCardInDiscard(data.Position.Rectangle.Center.ToVector2(), CropTemplate.Corn); })
    );
    
    public static CropTemplate Corn = new(
        "Corn", 
        6,
        AlwaysNever.Always,
        new CropLevel(5, 5),
        new OnHarvestTrigger(
            data => { Fx.GainEnergy(data.Position.Rectangle.Center.ToVector2(), 50); })
    );
    
    public static CropTemplate Watermelon = new(
        "Watermelon", 
        5,
        AlwaysNever.Always,
        new CropLevel(4, 10),
        new OnHarvestTrigger(
            data => { /*Water adjacent crops*/ })
    );

    public bool IsRecyclable => RecycleStatus == AlwaysNever.Always;

    public int EffectiveMaxLevel => CropLevel.NumberOfFrames - 1;

    public Crop CreateCrop(CropEventData data)
    {
        var crop = new Crop(data);

        crop.Harvested += CropTriggers.OnHarvest;
        crop.Grew += CropTriggers.OnGrow;
        crop.FinishedGrowing += CropTriggers.OnFinishedGrowing;

        CropTriggers.OnPlanted(data);

        return crop;
    }
}
