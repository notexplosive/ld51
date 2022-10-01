using ExTween;
using ExTweenMonoGame;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class Farmer : BaseComponent
{
    private SequenceTween _tween = new();
    private readonly TweenableVector2 _tweenablePosition;
    private float _speed = 400f;

    public Farmer(Actor actor) : base(actor)
    {
        _tweenablePosition = new TweenableVector2(()=>Transform.Position, val => Transform.Position = val);
    }

    public Tile? CurrentTile { get; set; }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }

    public void ClearTween()
    {
        _tween.Clear();
    }
    
    public void WalkTo(Vector2 target)
    {
        var distance = (_tweenablePosition.Value - target).Length();
        var duration = distance / _speed;

        if (distance == 0)
        {
            return;
        }
        
        _tween.Add(new Tween<Vector2>(_tweenablePosition, target, duration, Ease.Linear));
    }

    public void GoToTile(Tile tile)
    {
        CurrentTile = null;
        ClearTween();
        WalkTo(tile.Rectangle.Center.ToVector2());
        CurrentTile = tile;
    }
}
