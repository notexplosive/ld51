using ExplogineMonoGame;
using MachinaLite;

namespace LD51;

internal class GardenRenderer : BaseComponent
{
    private readonly Garden _garden;

    public GardenRenderer(Actor actor) : base(actor)
    {
        _garden = RequireComponent<Garden>();
    }

    public override void Draw(Painter painter)
    {
        foreach (var tilePosition in _garden.FilledPositions())
        {
            var crop = _garden.GetCropAt(tilePosition);
            var renderPos = tilePosition.Rectangle.Center.ToVector2();

            crop.Draw(painter, renderPos, Transform.Depth - 2 - tilePosition.GridPosition.Y);
        }
    }

    public override void Update(float dt)
    {
        foreach (var crop in _garden.AllCrops())
        {
            crop.Update(dt);
        }
    }
}
