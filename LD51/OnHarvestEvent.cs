namespace LD51;

public class OnHarvestEvent : CropEvents
{
    private readonly Crop.CropEvent _action;

    public OnHarvestEvent(Crop.CropEvent action)
    {
        _action = action;
    }

    public override void OnHarvest(TilePosition position, Garden garden)
    {
        _action(position, garden);
    }

    public override void OnGrow(TilePosition position, Garden garden)
    {
    }

    public override void OnFinishedGrowing(TilePosition position, Garden garden)
    {
    }
}
