namespace LD51;

public record NormalGrowCondition(int TickLength) : ICropGrowCondition
{
    public bool CanGrow(CropEventData data)
    {
        return true;
    }

    public string Description()
    {
        return $"Grows every {TickLength} seconds";
    }
}
