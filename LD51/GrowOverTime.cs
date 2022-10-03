namespace LD51;

public record GrowOverTime(int TickLength) : ICropGrowCondition
{
    public bool CanGrow(CropEventData data, float time)
    {
        return time > TickLength;
    }

    public string Description()
    {
        return $"Grows every {TickLength} seconds";
    }

    public string Progress(float time)
    {
        return $"\n{(int) (time / TickLength * 100)}%";
    }
}
