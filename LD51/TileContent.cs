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

    public static readonly TileContent Dirt = new(0, ContentTrait.Dirt, "Till", "Dirt");
    public static readonly TileContent Tilled = new(3, ContentTrait.Tilled, "Water", "Dry Soil");
    public static readonly TileContent WateredL3 = new(4, ContentTrait.Wet, "Water", "Wet Soil (3)");
    public static readonly TileContent WateredL2 = new(5, ContentTrait.Wet, "Water", "Wet Soil (2)");
    public static readonly TileContent WateredL1 = new(6, ContentTrait.Wet, "Water", "Wet Soil (1)");
    public static readonly TileContent Dead = new(2, ContentTrait.Dead, "Revive", "Dead Soil");
    public static readonly TileContent Dying = new(1, ContentTrait.Dead, "Revive", "Dead Soil");

    private TileContent(int frame, ContentTrait trait, string upgradeVerb, string name)
    {
        Frame = frame;
        Trait = trait;
        UpgradeVerb = upgradeVerb;
        Name = name;
    }

    public bool IsTilled => Trait == ContentTrait.Tilled || Trait == ContentTrait.Wet;

    public string Name { get; }
    public ContentTrait Trait { get; }
    public int Frame { get; }
    public bool IsWet => Trait == ContentTrait.Wet;
    public bool IsDead => Trait == ContentTrait.Dead;
    public string UpgradeVerb { get; }

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

    public int UpgradeCost()
    {
        if (this == TileContent.Dirt)
        {
            return A.TillCost;
        }

        if (this == TileContent.Tilled || IsWet)
        {
            return A.WaterCost;
        }

        return 0;
    }
}
