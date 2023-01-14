using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class SpriteRenderer : BaseComponent
{
    private readonly SpriteSheet _spriteSheet;
    private IFrameAnimation _currentAnimation;
    private float _elapsedTime;

    public SpriteRenderer(Actor actor, SpriteSheet spriteSheet) : base(actor)
    {
        _spriteSheet = spriteSheet;
        _currentAnimation = spriteSheet.DefaultAnimation;
        Color = Color.White;
    }

    public DrawOrigin Origin { get; set; } = DrawOrigin.Center;
    public Color Color { get; set; }
    public float Scale { get; set; } = 1f;
    public int FramesPerSecond { get; set; } = 15;

    public bool IsPaused { get; set; }
    public XyBool Flip { get; set; }

    public int CurrentFrame => _currentAnimation.GetFrame(_elapsedTime);

    public override void Draw(Painter painter)
    {
        _spriteSheet.DrawFrameAtPosition(painter, CurrentFrame, Transform.Position, new Scale2D(Scale),
            new DrawSettings
                {Color = Color, Flip = Flip, Angle = Transform.Angle, Depth = Transform.Depth, Origin = Origin});
    }

    public override void Update(float dt)
    {
        if (!IsPaused)
        {
            IncrementTime(dt);
        }
    }

    public void SetFrame(int frame)
    {
        _elapsedTime = frame;
    }

    public bool IsAnimationFinished()
    {
        return _elapsedTime > _currentAnimation.Length;
    }

    public SpriteRenderer SetAnimation(IFrameAnimation animation)
    {
        if (!_currentAnimation.Equals(animation))
        {
            _elapsedTime = 0;
            _currentAnimation = animation;
        }

        return this;
    }

    private void IncrementTime(float dt)
    {
        SetElapsedTime(_elapsedTime + dt * FramesPerSecond);
    }

    private void SetElapsedTime(float newTime)
    {
        _elapsedTime = newTime;
    }
}
