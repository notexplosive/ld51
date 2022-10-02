using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using MachinaLite;

namespace LD51;

public class SelectedTileRenderer : BaseComponent
{
    private readonly Farmer _farmer;
    private readonly Tiles _tiles;

    public SelectedTileRenderer(Actor actor, Farmer farmer) : base(actor)
    {
        _tiles = RequireComponent<Tiles>();
        _farmer = farmer;
    }

    public override void Draw(Painter painter)
    {
        if (_tiles.HoveredTile.HasValue && !_farmer.IsAnimating)
        {
            var rect = _tiles.HoveredTile.Value.Rectangle;
            rect.Inflate(-10, -10);
            painter.DrawRectangle(rect,
                new DrawSettings
                {
                    Color = LudumCartridge.World
                        .GetTapAction(_tiles.HoveredTile.Value, LudumCartridge.Ui.Inventory.GrabbedCard)
                        .Color.WithMultipliedOpacity(0.25f),
                    Depth = Transform.Depth - 1
                });
        }
    }
}
