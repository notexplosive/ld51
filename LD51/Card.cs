using System;
using ExplogineMonoGame.HitTesting;
using ExplogineMonoGame.Input;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

internal class Card : BaseComponent
{
    private readonly Hoverable _hoverable;
    private readonly SeedInventory _inventory;

    public Card(Actor actor, SeedInventory inventory, Crop crop) : base(actor)
    {
        _hoverable = RequireComponent<Hoverable>();
        _inventory = inventory;
        Crop = crop;
    }

    public float HoverTimer { get; private set; }
    public Crop Crop { get; }

    public override void Update(float dt)
    {
        if (_inventory.IsGrabbed(this))
        {
            if (_hoverable.IsHovered)
            {
                HoverTimer += dt * 5;
            }
            else
            {
                HoverTimer -= dt * 10;
            }

            HoverTimer = Math.Clamp(HoverTimer, 0f, 1f);

            Transform.LocalPosition = new Vector2(Transform.LocalPosition.X, -HoverTimer * 40f);
        }
        else
        {
            Transform.LocalPosition = new Vector2(Transform.LocalPosition.X, -80);
        }
    }

    public override void OnMouseButton(MouseButton button, Vector2 currentPosition, ButtonState state,
        HitTestStack hitTestStack)
    {
        if (_hoverable.IsHovered && button == MouseButton.Left)
        {
            if (state == ButtonState.Pressed)
            {
                _inventory.Grab(this);
            }
        }
    }
}