namespace LD51;

public static class PlayerStats
{
    public static Stat Energy { get; } = new();

    public class Stat
    {
        public int Amount { get; set; }

        public bool CanAfford(int cost)
        {
            return Amount >= cost;
        }

        public void Consume(int cost)
        {
            Amount -= cost;
        }

        public void Gain(int gain)
        {
            Amount += gain;
        }
    }
}
