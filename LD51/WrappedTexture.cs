using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51;

public class WrappedTexture : BaseComponent
{
    private readonly Box _box;
    private readonly Texture2D _texture;

    public WrappedTexture(Actor actor, Texture2D texture2D) : base(actor)
    {
        _box = RequireComponent<Box>();
        _texture = texture2D;
    }

    public override void Draw(Painter painter)
    {
        painter.DrawAsRectangle(_texture, _box.RectangleWithoutOffset,
            new DrawSettings
            {
                Color = Color.White, Angle = Transform.Angle, Depth = Transform.Depth,
                Origin = new DrawOrigin(_box.Offset.ToVector2()),
                SourceRectangle = _box.RectangleAtOrigin
            });
    }
}
