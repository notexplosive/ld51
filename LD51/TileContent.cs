namespace LD51;

public class TileContent
{
    public enum ContentTrait
    {
        Dirt,
        Tilled,
        Wet,
        Dead
    }

    public static readonly TileContent Dirt = new(0, ContentTrait.Dirt);
    public static readonly TileContent Tilled = new(3, ContentTrait.Tilled);
    public static readonly TileContent WateredL3 = new(4, ContentTrait.Wet);
    public static readonly TileContent WateredL2 = new(5, ContentTrait.Wet);
    public static readonly TileContent WateredL1 = new(6, ContentTrait.Wet);
    public static readonly TileContent Dead = new(2, ContentTrait.Dead);
    public static readonly TileContent Dying = new(1, ContentTrait.Dead);

    private TileContent(int frame, ContentTrait trait)
    {
        Frame = frame;
        Trait = trait;
    }

    public ContentTrait Trait { get; }

    public int Frame { get; }
    public bool IsWet => Trait == ContentTrait.Wet;

    public TileContent Upgrade()
    {
        if (this == TileContent.Dirt)
        {
            return TileContent.Tilled;
        }

        if (this == TileContent.Tilled || IsWet)
        {
            return TileContent.WateredL3;
        }

        return this;
    }

    public TileContent Downgrade()
    {
        if (this == TileContent.WateredL3)
        {
            return TileContent.WateredL2;
        }

        if (this == TileContent.WateredL2)
        {
            return TileContent.WateredL1;
        }

        if (this == TileContent.WateredL1)
        {
            return TileContent.Tilled;
        }

        return this;
    }
}
