namespace LD51;

public static class PlayerStats
{
    public static Stat Energy { get; } = new();

    public class Stat
    {
        public int Amount { get; set; }
    }
}
