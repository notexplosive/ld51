using System;
using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;

namespace LD51;

public enum Rarity
{
    Starter,
    Common,
    Rare,
    Legendary
}

public record CropTemplate(string Name, ICropGrowCondition GrowCondition, Rarity Rarity,
    CropGraphic CropGraphic, CropBehaviors CropBehaviors)
{
    public int EffectiveMaxLevel => CropGraphic.NumberOfGrows;

    public string Description
    {
        get
        {
            var strings = new List<string>();

            strings.Add(GrowCondition.Description());
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

    public static CropTemplate GetRandomOfRarity(Rarity rarity, CropTemplate excluding = null)
    {
        var templates = new List<CropTemplate>();
        foreach (var template in CropTemplate.AllTemplates)
        {
            if (template.Rarity == rarity)
            {
                templates.Add(template);
            }
        }

        var result = Client.Random.Clean.GetRandomElement(templates);

        while (result == excluding)
        {
            result = Client.Random.Clean.GetRandomElement(templates);
        }

        return result;
    }

    public static List<CropTemplate> AllTemplates = CropTemplate.GenerateAllTemplates().ToList();

    public static IEnumerable<CropTemplate> GenerateAllTemplates()
    {
        // CASH
        
        yield return new CropTemplate(
            "Collard",
            new GrowOverTime(3),
            Rarity.Common,
            new CropGraphic(2, 14),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainEnergy(20))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        yield return new CropTemplate(
            "Spinach",
            new GrowOverTime(3),
            Rarity.Rare,
            new CropGraphic(2, 17),
            new CropBehaviors()
                .WhenGrow(CropActivity.GainEnergy(5))
                .WhenHarvested(CropActivity.GainEnergy(25))
                .WhenHarvested(CropActivity.Recycle())
        );

        yield return new CropTemplate(
            "Parsnip",
            new GrowOverTime(5),
            Rarity.Rare,
            new CropGraphic(2, 20),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainEnergy(50))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        // RESEARCH

        yield return new CropTemplate(
            "Carrot",
            new GrowOverTime(10),
            Rarity.Common,
            new CropGraphic(1, 3),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainCardOfRarity(Rarity.Common, "Carrot"))
                .WhenHarvested(CropActivity.GainEnergy(15))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        yield return new CropTemplate(
            "Beet",
            new GrowOverTime(10),
            Rarity.Common,
            new CropGraphic(1, 23),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainCardOfRarity(Rarity.Rare, "Beet"))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        /*
        yield return new CropTemplate(
            "Cauliflower",
            10,
            Rarity.Rare,
            new CropGraphic(1, 3),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainCardOfRarity(Rarity.Epic))
                .WhenHarvested(CropActivity.Recycle())
        );
        */
        
        yield return new CropTemplate(
            "Kohlrabi",
            new GrowOverTime(10),
            Rarity.Rare,
            new CropGraphic(1, 25),
            new CropBehaviors()
                .WhenHarvested(CropActivity.GainCardOfRarity(Rarity.Legendary))
        );
        
        // UTILITY

        yield return new CropTemplate(
            "Watermelon",
            new GrowOverTime(1),
            Rarity.Rare,
            new CropGraphic(3, 10),
            new CropBehaviors()
                .WhenHarvested(CropActivity.ForceAdjacentTilesToBeWatered())
                .WhenHarvested(CropActivity.Recycle())
        );
        
        // yield return new CropTemplate(
        //     "Potato",
        //     new NormalGrowCondition(5),
        //     Rarity.Common,
        //     new CropGraphic(3, 27),
        //     new CropBehaviors()
        //         .WhenHarvested(CropActivity.TillAdjacentCrops())
        //         .WhenHarvested(CropActivity.Recycle())
        // );
        //
        // yield return new CropTemplate(
        //     "Corn",
        //     new NormalGrowCondition(5),
        //     Rarity.Common,
        //     new CropGraphic(3, 5),
        //     new CropBehaviors()
        //         .WhenHarvested(CropActivity.RestoreAdjacentCrops())
        //         .WhenHarvested(CropActivity.Recycle())
        // );
        
        // CARD DRAW
        
        yield return new CropTemplate(
            "Onion",
            new GrowOverTime(2),
            Rarity.Common,
            new CropGraphic(2, 31),
            new CropBehaviors()
                .WhenHarvested(CropActivity.DrawCard(2))
                .WhenHarvested(CropActivity.GainEnergy(25))
                .WhenHarvested(CropActivity.Recycle())
        );
        
        // yield return new CropTemplate(
        //     "Radish",
        //     2,
        //     Rarity.Rare,
        //     new CropGraphic(2, 3),
        //     new CropBehaviors()
        //         .WhenHarvested(CropActivity.DrawCard(2))
        //         .WhenHarvested(CropActivity.Recycle())
        // );
        
        // SPECIAL
        
        yield return new CropTemplate(
            "Pumpkin",
            new GrowOverTime(10), // todo
            Rarity.Legendary,
            new CropGraphic(3, 34),
            new CropBehaviors()
                .WhenHarvested(CropActivity.WinGame())
        );
        
        // yield return new CropTemplate(
        //     "Turnip",
        //     5,
        //     Rarity.Legendary,
        //     new CropGraphic(3, 10),
        //     new CropBehaviors()
        //         .WhenHarvested(CropActivity.ReShuffleForFree())
        // );
        
        // yield return new CropTemplate(
        //     "Bok Choy",
        //     5,
        //     Rarity.Rare,
        //     new CropGraphic(2, 10),
        //     new CropBehaviors()
        //         .WhenHarvested(CropActivity.GrowAdjacentCrops())
        //         .WhenHarvested(CropActivity.Recycle())
        // );
        
        // yield return new CropTemplate(
        //     "Cabbage",
        //     5,
        //     Rarity.Rare,
        //     new CropGraphic(2, 10),
        //     new CropBehaviors()
        //         .WhenHarvested(CropActivity.DoubleHarvestAdjacentCrops())
        //         .WhenHarvested(CropActivity.Recycle())
        // );
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