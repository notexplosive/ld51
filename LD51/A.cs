using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD51;

public static class A
{
    public static SpriteSheet TileSheet { get; set; }
    public static Rectangle TileRect { get; set; }
    public static Point CardSize => new Point(150, 300);
    public static int DrawCardCost => 10;
    public static Font CardTextFont => Client.Assets.GetFont("GameFont", 32);
}
