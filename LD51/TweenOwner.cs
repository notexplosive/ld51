using ExTween;
using MachinaLite;

namespace LD51;

public class TweenOwner : BaseComponent
{
    public ITween Tween { get; set; }

    public TweenOwner(Actor actor) : base(actor)
    {
        Tween = new SequenceTween();
    }

    public override void Update(float dt)
    {
        Tween.Update(dt);
    }
}
