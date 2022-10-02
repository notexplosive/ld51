using System.Collections.Generic;

namespace LD51;

public class CropBehaviors
{
    public CropActivityCollection Harvested { get; } = new("Harvest");
    public CropActivityCollection Grow { get; } = new("Grow");
    public CropActivityCollection FinishedGrow { get; } = new("Finished growing");
    public CropActivityCollection Planted { get; } = new("Planted");

    public IEnumerable<CropActivityCollection> AllActivities()
    {
        yield return Planted;
        yield return Grow;
        yield return FinishedGrow;
        yield return Harvested;
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
}
