using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class NinepatchRenderer : BaseComponent
{
    private readonly NinepatchSheet _sheet;
    private readonly Box _box;

    public NinepatchRenderer(Actor actor, NinepatchSheet sheet) : base(actor)
    {
        _box = RequireComponent<Box>();
        _sheet = sheet;
    }

    public override void Draw(Painter painter)
    {
        var rect = _box.Rectangle;
        rect.Offset(Offset);
        _sheet.DrawFullNinepatch(painter, rect, InnerOuter.Inner, Transform.Depth, 1f);
    }

    public Point Offset { get; set; }
}
