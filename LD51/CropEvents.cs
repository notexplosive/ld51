namespace LD51;

public abstract class CropEvents
{
    public abstract void OnHarvest(TilePosition position, Garden garden);
    public abstract void OnGrow(TilePosition position, Garden garden);
    public abstract void OnFinishedGrowing(TilePosition position, Garden garden);
}
