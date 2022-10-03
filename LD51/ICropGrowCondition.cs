namespace LD51;

public interface ICropGrowCondition
{
    bool CanGrow(CropEventData data, float time);
    string Description();
    string Progress(float time);
}
