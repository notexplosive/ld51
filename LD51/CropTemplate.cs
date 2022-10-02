using System.Collections.Generic;

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

public record CropTemplate(string Name, int TickLength, Rarity Rarity, AlwaysNever RecycleStatus,
    CropGraphic CropGraphic, CropBehaviors CropBehaviors)
{
    public static CropTemplate Potato = new(
        "Potato",
        10,
        Rarity.Common,
        AlwaysNever.Always,
        new CropGraphic(3, 0),
        new CropBehaviors()
            .WhenHarvested(CropActivity.GainEnergy(20))
    );

    public static CropTemplate Corn = new(
        "Corn",
        6,
        Rarity.Common,
        AlwaysNever.Always,
        new CropGraphic(5, 5),
        new CropBehaviors()
            .WhenHarvested(CropActivity.GainEnergy(50))
    );

    public static CropTemplate Carrot = new(
        "Carrot",
        10,
        Rarity.Common,
        AlwaysNever.Always,
        new CropGraphic(2, 3),
        new CropBehaviors()
            .WhenHarvested(CropActivity.GainCard(CropTemplate.Corn))
    );

    public static CropTemplate Watermelon = new(
        "Watermelon",
        5,
        Rarity.Rare,
        AlwaysNever.Always,
        new CropGraphic(4, 10),
        new CropBehaviors()
            .WhenHarvested(CropActivity.WaterAdjacentCrops())
    );

    public bool IsRecyclable => RecycleStatus == AlwaysNever.Always;

    public int EffectiveMaxLevel => CropGraphic.NumberOfFrames - 1;

    public string Description
    {
        get
        {
            var strings = new List<string>();
            foreach (var activity in CropBehaviors.AllActivities())
            {
                var desc = activity.Description();
                if (!string.IsNullOrEmpty(desc))
                {
                    strings.Add(desc);
                }
            }

            return string.Join("\n", strings);
        }
    }

    public Crop CreateCrop(CropEventData data)
    {
        var crop = new Crop(data);

        crop.Harvested += CropBehaviors.Harvested.Run;
        crop.Grew += CropBehaviors.Grow.Run;
        crop.FinishedGrowing += CropBehaviors.FinishedGrow.Run;

        CropBehaviors.Planted.Run(data);

        return crop;
    }
}
