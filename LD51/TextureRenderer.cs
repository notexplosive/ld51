using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using MachinaLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51;

public class TextureRenderer : BaseComponent
{
    private readonly Texture2D _texture2d;
    private readonly DrawOrigin? _drawOrigin;

    public TextureRenderer(Actor actor, Texture2D texture2d, DrawOrigin? drawOrigin = null) : base(actor)
    {
        _texture2d = texture2d;
        _drawOrigin = drawOrigin;
    }

    public override void Draw(Painter painter)
    {
        painter.DrawAtPosition(_texture2d, Transform.Position, Scale2D.One,
            new DrawSettings
                {Angle = Transform.Angle, Color = Color.White, Depth = Transform.Depth, Origin = _drawOrigin ?? DrawOrigin.Center});
    }
}
