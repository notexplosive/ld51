using System.Collections.Generic;

namespace LD51;

public class CropBehaviors
{
    public CropActivityCollection Harvested { get; } = new("When Harvested");
    public CropActivityCollection Grow { get; } = new("On Grow");
    public CropActivityCollection FinishedGrow { get; } = new("When finished growing");
    public CropActivityCollection Planted { get; } = new("When planted");
    public CropActivityCollection PlantedAdjacent { get; } = new("When a neighboring crop is planted");

    public IEnumerable<CropActivityCollection> AllActivities()
    {
        yield return Planted;
        yield return Grow;
        yield return FinishedGrow;
        yield return Harvested;
        yield return PlantedAdjacent;
    }

    public CropBehaviors WhenHarvested(CropActivity activity)
    {
        Harvested.Add(activity);
        return this;
    }

    public CropBehaviors WhenGrow(CropActivity activity)
    {
        Grow.Add(activity);
        return this;
    }

    public CropBehaviors WhenFinishedGrowing(CropActivity activity)
    {
        FinishedGrow.Add(activity);
        return this;
    }

    public CropBehaviors WhenPlanted(CropActivity activity)
    {
        Planted.Add(activity);
        return this;
    }
    
    public CropBehaviors WhenPlantedAdjacent(CropActivity activity)
    {
        PlantedAdjacent.Add(activity);
        return this;
    }
}
