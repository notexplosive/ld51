using System.Collections.Generic;

namespace LD51;

public enum Rarity
{
    Common,
    Rare,
    Legendary
}

public record CropTemplate(string Name, int TickLength, Rarity Rarity,
    CropGraphic CropGraphic, CropBehaviors CropBehaviors)
{
    public static CropTemplate Potato = new(
        "Potato",
        10,
        Rarity.Common,
        new CropGraphic(3, 0),
        new CropBehaviors()
            .WhenHarvested(CropActivity.GainEnergy(20))
            .WhenHarvested(CropActivity.Recycle())
    );

    public static CropTemplate Corn = new(
        "Corn",
        6,
        Rarity.Common,
        new CropGraphic(5, 5),
        new CropBehaviors()
            .WhenHarvested(CropActivity.GainEnergy(50))
            .WhenHarvested(CropActivity.Recycle())
    );

    public static CropTemplate Carrot = new(
        "Carrot",
        10,
        Rarity.Common,
        new CropGraphic(2, 3),
        new CropBehaviors()
            .WhenHarvested(CropActivity.GainCard(CropTemplate.Corn))
            .WhenHarvested(CropActivity.Recycle())
    );

    public static CropTemplate Watermelon = new(
        "Watermelon",
        5,
        Rarity.Rare,
        new CropGraphic(4, 10),
        new CropBehaviors()
            .WhenHarvested(CropActivity.WaterAdjacentCrops())
            .WhenHarvested(CropActivity.Recycle())
    );

    public int EffectiveMaxLevel => CropGraphic.NumberOfFrames - 1;

    public string Description
    {
        get
        {
            var strings = new List<string>();
            
            strings.Add($"Grows every {TickLength} seconds");
            strings.Add($"Takes {EffectiveMaxLevel} Grows to harvest");
            
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
