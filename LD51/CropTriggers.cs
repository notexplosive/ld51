namespace LD51;

public abstract class CropTriggers
{
    public abstract void OnHarvest(CropEventData data);
    public abstract void OnGrow(CropEventData data);
    public abstract void OnFinishedGrowing(CropEventData data);
    public abstract void OnPlanted(CropEventData data);
}
