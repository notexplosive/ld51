namespace LD51;

public enum AlwaysNever
{
    Always,
    Never
}

public enum Rarity
{
    Common,
    Rare,
    Legendary
}

public record CropTemplate(string Name, int TickLength, Rarity Rarity, AlwaysNever RecycleStatus, CropGraphic CropGraphic, CropTriggers CropTriggers)
{
    public static CropTemplate Potato = new(
        "Potato", 
        10, 
        Rarity.Common,
        AlwaysNever.Always,
        new CropGraphic(3, 0),
        new OnHarvestTrigger(
            data => { Fx.GainEnergy(data.Position.Rectangle.Center.ToVector2(), 15); })
        );
    
    public static CropTemplate Carrot = new(
        "Carrot", 
        10,
        Rarity.Common,
        AlwaysNever.Always,
        new CropGraphic(2, 3),
        new OnHarvestTrigger(
            data => { Fx.PutCardInDiscard(data.Position.Rectangle.Center.ToVector2(), CropTemplate.Corn); })
    );
    
    public static CropTemplate Corn = new(
        "Corn", 
        6,
        Rarity.Common,
        AlwaysNever.Always,
        new CropGraphic(5, 5),
        new OnHarvestTrigger(
            data => { Fx.GainEnergy(data.Position.Rectangle.Center.ToVector2(), 50); })
    );
    
    public static CropTemplate Watermelon = new(
        "Watermelon", 
        5,
        Rarity.Rare,
        AlwaysNever.Always,
        new CropGraphic(4, 10),
        new OnHarvestTrigger(
            data => { /*Water adjacent crops*/ })
    );

    public bool IsRecyclable => RecycleStatus == AlwaysNever.Always;

    public int EffectiveMaxLevel => CropGraphic.NumberOfFrames - 1;
    public string Description { get; }

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
