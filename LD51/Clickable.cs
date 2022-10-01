using ExplogineMonoGame.HitTesting;
using ExplogineMonoGame.Input;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class Clickable : BaseComponent
{
    private readonly Hoverable _hoverable;

    public Clickable(Actor actor) : base(actor)
    {
        _hoverable = RequireComponent<Hoverable>();
    }

    public override void OnMouseButton(MouseButton button, Vector2 currentPosition, ButtonState state, HitTestStack hitTestStack)
    {
        if (state == ButtonState.Pressed && _hoverable.IsHovered)
        {
            Clicked?.Invoke(button);
        }
    }

    public delegate void ClickEvent(MouseButton button);
    public event ClickEvent Clicked;
}
