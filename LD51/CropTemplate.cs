using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;

namespace LD51;

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public record CropTemplate(string Name, int TickLength, Rarity Rarity,
    CropGraphic CropGraphic, CropBehaviors CropBehaviors)
{
    public int EffectiveMaxLevel => CropGraphic.NumberOfGrows;

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

    public static CropTemplate GetRandomOfRarity(Rarity rarity)
    {
        var templates = new List<CropTemplate>();
        foreach (var template in CropTemplate.AllTemplates)
        {
            if (template.Rarity == rarity)
            {
                templates.Add(template);
            }
        }

        return Client.Random.Clean.GetRandomElement(templates);
    }

    public static List<CropTemplate> AllTemplates = CropTemplate.GenerateAllTemplates().ToList();

    public static IEnumerable<CropTemplate> GenerateAllTemplates()
    {
        // CASH
        
        yield return new CropTemplate(
            "Collard",
            5,
            Rarity.Common,
            new CropGraphic(2, 0),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainEnergy(20))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        yield return new CropTemplate(
            "Spinach",
            3,
            Rarity.Rare,
            new CropGraphic(3, 0),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainEnergy(5))
                .WhenHarvested(CropActivity.Recycle())
                .WhenGrow(CropActivity.GainEnergy(5))
        );

        yield return new CropTemplate(
            "Parsnip",
            5,
            Rarity.Rare,
            new CropGraphic(2, 5),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainEnergy(50))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        // RESEARCH

        yield return new CropTemplate(
            "Carrot",
            10,
            Rarity.Common,
            new CropGraphic(1, 3),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainCardOfRarity(Rarity.Common))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        yield return new CropTemplate(
            "Beet",
            10,
            Rarity.Common,
            new CropGraphic(1, 3),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainCardOfRarity(Rarity.Rare))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        yield return new CropTemplate(
            "Cauliflower",
            10,
            Rarity.Rare,
            new CropGraphic(1, 3),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainCardOfRarity(Rarity.Epic))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        yield return new CropTemplate(
            "Kohlrabi",
            10,
            Rarity.Rare,
            new CropGraphic(1, 3),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainCardOfRarity(Rarity.Legendary))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        // UTILITY

        yield return new CropTemplate(
            "Watermelon",
            5,
            Rarity.Common,
            new CropGraphic(3, 10),
            new CropBehaviors()
                .WhenHarvested(CropActivity.WaterAdjacentCrops())
                .WhenHarvested(CropActivity.Recycle())
        );
        
        yield return new CropTemplate(
            "Potato",
            5,
            Rarity.Rare,
            new CropGraphic(3, 10),
            new CropBehaviors()
                .WhenHarvested(CropActivity.TillAdjacentCrops())
                .WhenHarvested(CropActivity.Recycle())
        );
        
        yield return new CropTemplate(
            "Corn",
            5,
            Rarity.Rare,
            new CropGraphic(3, 10),
            new CropBehaviors()
                .WhenHarvested(CropActivity.RestoreAdjacentCrops())
                .WhenHarvested(CropActivity.Recycle())
        );
        
        // CARD DRAW
        
        yield return new CropTemplate(
            "Onion",
            6,
            Rarity.Rare,
            new CropGraphic(1, 3),
            new CropBehaviors()
                .WhenHarvested(CropActivity.DrawCard())
                .WhenHarvested(CropActivity.Recycle())
        );
        
        // SPECIAL
        
        yield return new CropTemplate(
            "Turnip",
            5,
            Rarity.Legendary,
            new CropGraphic(3, 10),
            new CropBehaviors()
                .WhenHarvested(CropActivity.ReShuffleForFree())
        );
        
        yield return new CropTemplate(
            "Bok Choy",
            5,
            Rarity.Epic,
            new CropGraphic(2, 10),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GrowAdjacentCrops())
        );
        
        yield return new CropTemplate(
            "Cabbage",
            5,
            Rarity.Epic,
            new CropGraphic(2, 10),
            new CropBehaviors()
                .WhenHarvested(CropActivity.DoubleHarvestAdjacentCrops())
        );
    }

    public static CropTemplate GetByName(string name)
    {
        foreach (var crop in CropTemplate.AllTemplates)
        {
            if (crop.Name == name)
            {
                return crop;
            }
        }

        throw new Exception($"Could not find crop by name: {name}");
    }
}
