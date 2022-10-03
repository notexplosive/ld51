namespace LD51;

public interface ICropGrowCondition
{
    public int TickLength { get; }
    bool CanGrow(CropEventData data);
    string Description();
}
