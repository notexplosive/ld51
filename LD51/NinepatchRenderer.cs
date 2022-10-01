using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using MachinaLite;
using MachinaLite.Components;

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
        _sheet.DrawFullNinepatch(painter, _box.Rectangle, InnerOuter.Inner, Transform.Depth, 1f);
    }
}
