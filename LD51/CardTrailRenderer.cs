using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class CardTrailRenderer : BaseComponent
{
    private readonly TweenableFloat _x = new();
    private readonly TweenableFloat _y = new();
    private CardAndTile _cardAndTile;
    private float _time;
    private ITween _tween = new SequenceTween();

    public CardTrailRenderer(Actor actor) : base(actor)
    {
    }

    public override void Draw(Painter painter)
    {
        if (_tween != null && _cardAndTile != null && _cardAndTile.Tile.HasValue)
        {
            for (float f = 0; f < 1f; f += 0.1f)
            {
                _tween.JumpTo((f + _time) % 1f);

                painter.DrawRectangle(new Rectangle(new Point((int) _x, (int) _y), new Point(10)),
                    new DrawSettings
                    {
                        Color = LudumCartridge.World.GetTapAction(_cardAndTile.Tile.Value, _cardAndTile.Card).Color,
                        Origin = DrawOrigin.Center
                    });
            }
        }
    }

    public override void Update(float dt)
    {
        _time += dt / 5f;

        var card = LudumCartridge.Ui.Inventory.GrabbedCard;
        var hoveredTile = LudumCartridge.World.Tiles.HoveredTile;
        var newCardAndTitle = new CardAndTile(card, hoveredTile);
        if (card != null && hoveredTile.HasValue)
        {
            if (_cardAndTile != newCardAndTitle)
            {
                _tween = newCardAndTitle.MakeTween(_x, _y);
            }
        }
        else
        {
            _tween = null;
        }

        _cardAndTile = newCardAndTitle;
    }

    public record CardAndTile(Card Card, TilePosition? Tile)
    {
        public ITween MakeTween(TweenableFloat x, TweenableFloat y)
        {
            var source = Card.Rectangle.Center.ToVector2();
            var destination = Vector2.Zero;
            if (Tile.HasValue)
            {
                destination = Tile.Value.Rectangle.Center.ToVector2();
            }

            x.Value = source.X;
            y.Value = source.Y;

            return new MultiplexTween()
                    .AddChannel(new Tween<float>(x, destination.X, 1f, Ease.SineSlowFast))
                    .AddChannel(new Tween<float>(y, destination.Y, 0.5f, Ease.SineFastSlow))
                ;
        }
    }
}
