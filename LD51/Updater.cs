using System;
using MachinaLite;

namespace LD51;

public class Updater : BaseComponent
{
    private readonly Action<float> _onUpdate;

    public Updater(Actor actor, Action<float> onUpdate) : base(actor)
    {
        _onUpdate = onUpdate;
    }

    public override void Update(float dt)
    {
        _onUpdate(dt);
    }
}
