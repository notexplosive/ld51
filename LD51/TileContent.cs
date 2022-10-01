namespace LD51;

public class TileContent
{
    public static readonly TileContent Normal = new(0);
    public static readonly TileContent Tilled = new(3);
    public static readonly TileContent Watered = new(4);
    public static readonly TileContent Dead = new(2);
    public static readonly TileContent Dying = new(1);

    private TileContent(int frame)
    {
        Frame = frame;
    }

    public int Frame { get; }

    public TileContent Upgrade()
    {
        if (this == TileContent.Normal)
        {
            return TileContent.Tilled;
        }

        if (this == TileContent.Tilled)
        {
            return TileContent.Watered;
        }

        return this;
    }
}
