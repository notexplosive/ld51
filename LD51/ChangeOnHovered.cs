using System;
using MachinaLite;
using MachinaLite.Components;

namespace LD51;

public class ChangeOnHovered : BaseComponent
{
    private readonly Action _whenHovered;
    private readonly Action _whenNotHovered;
    private readonly Hoverable _hoverable;

    public ChangeOnHovered(Actor actor, Action whenHovered, Action whenNotHovered) : base(actor)
    {
        _whenHovered = whenHovered;
        _whenNotHovered = whenNotHovered;
        _hoverable = RequireComponent<Hoverable>();
    }

    public override void Update(float dt)
    {
        if (_hoverable.IsHovered)
        {
            _whenHovered();
        }
        else
        {
            _whenNotHovered();
        }
    }
}
