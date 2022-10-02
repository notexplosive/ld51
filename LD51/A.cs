using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD51;

public static class A
{
    public static SpriteSheet TileSheet { get; set; }
    public static Rectangle TileRect { get; set; }
    public static Point CardSize => new(150, 300);
    public static int DrawCardCost => 10;
    public static Font CardTextFont => Client.Assets.GetFont("GameFont", 32);
    public static Font TooltipTextFont => Client.Assets.GetFont("GameFont", 32);
    public static Font TooltipTitleFont => Client.Assets.GetFont("GameFont", 50);
    public static Font UiHintFont => Client.Assets.GetFont("GameFont", 24);
    public static Font BigFont => Client.Assets.GetFont("GameFont", 64);
    public static Font TitleScreenFont => Client.Assets.GetFont("GameFont", 128);
    public static readonly int TillCost = 15;
    public static readonly int WaterCost = 5;
}
