using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class CardRenderer : BaseComponent
{
    private readonly Box _box;
    private readonly float _opacity;
    private readonly CropTemplate _template;

    public CardRenderer(Actor actor, CropTemplate template) : base(actor)
    {
        _box = RequireComponent<Box>();
        _template = template;
        _opacity = 1f;
    }

    public override void Draw(Painter painter)
    {
        CardRenderer.DrawCard(painter, _box.Rectangle, _template, _opacity, 1f, Transform.Depth);
    }

    public static void DrawCard(Painter painter, Rectangle rectangle, CropTemplate template, float opacity,
        float textScale, Depth depth)
    {
        var ninePatchSheet = Client.Assets.GetAsset<NinepatchSheet>("Card-Patch");
        ninePatchSheet.DrawFullNinepatch(painter, rectangle, InnerOuter.Inner, depth, opacity);

        
        
        painter.DrawStringWithinRectangle(A.CardTextFont.WithFontSize((int) (A.CardTextFont.FontSize * textScale)),
            template.Name, rectangle, Alignment.Center,
            new DrawSettings {Depth = depth - 1, Color = Color.Black.WithMultipliedOpacity(opacity)});


        var insetRectangle = rectangle;
        insetRectangle.Inflate(-20, -20);
        painter.DrawStringWithinRectangle(A.CardTextFont.WithFontSize((int) (A.CardTextFont.FontSize * textScale)),
            template.Rarity.ToString(), insetRectangle, Alignment.TopCenter,
            new DrawSettings {Depth = depth - 1, Color = Color.Black.WithMultipliedOpacity(opacity)});
    }
}
