using ExplogineMonoGame;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class DummyCardRenderer : BaseComponent
{
    public float Opacity { get; set; } = 1f;
    public Rectangle Rectangle => new(Transform.Position.ToPoint(), (Size * Scale).ToPoint());
    public float Scale { get; set; } = 1f;
    public Vector2 Size => A.CardSize.ToVector2();
    public CropTemplate Template { get; set; }

    public DummyCardRenderer(Actor actor, CropTemplate template) : base(actor)
    {
        Template = template;
    }

    public override void Draw(Painter painter)
    {
        var rect = Rectangle;
        rect.Location -= (rect.Size.ToVector2() / 2).ToPoint();
        CardRenderer.DrawCard(painter, rect, Template, Opacity, Scale, Transform.Depth);
    }
}
