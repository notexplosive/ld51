namespace LD51;

public class OnHarvestTrigger : CropTriggers
{
    private readonly Crop.CropEvent _action;

    public OnHarvestTrigger(Crop.CropEvent action)
    {
        _action = action;
    }

    public override void OnHarvest(CropEventData data)
    {
        _action(data);
    }

    public override void OnGrow(CropEventData data)
    {
    }

    public override void OnFinishedGrowing(CropEventData data)
    {
    }
}
