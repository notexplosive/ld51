using System;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExTween;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class Card : BaseComponent
{
    private readonly Hoverable _hoverable;
    private readonly Inventory _inventory;
    private readonly Box _box;

    public Card(Actor actor, Inventory inventory, CropTemplate cropTemplate) : base(actor)
    {
        _box = RequireComponent<Box>();
        _hoverable = RequireComponent<Hoverable>();
        _inventory = inventory;
        CropTemplate = cropTemplate;
        HoverTimer = 1f;
    }

    public float HoverTimer { get; private set; }
    public CropTemplate CropTemplate { get; }
    public Rectangle Rectangle => _box.Rectangle;

    public override void Update(float dt)
    {
        if (!_inventory.IsGrabbed(this))
        {
            if (_hoverable.IsHovered)
            {
                HoverTimer += dt * 10;
            }
            else
            {
                HoverTimer -= dt * 20;
            }

            HoverTimer = Math.Clamp(HoverTimer, 0f, 1f);

            Transform.LocalPosition = new Vector2(Transform.LocalPosition.X, -Ease.CubicFastSlow(HoverTimer) * 60f);
        }
        else
        {
            Transform.LocalPosition = new Vector2(Transform.LocalPosition.X, -70);
        }

        if (_hoverable.IsHovered)
        {
            LudumCartridge.Ui.Tooltip.Set($"{CropTemplate.Name}", $"{CropTemplate.Description}");
        }
    }

    public override void OnMouseButton(MouseButton button, Vector2 currentPosition, ButtonState state,
        HitTestStack hitTestStack)
    {
        if (_hoverable.IsHovered && button == MouseButton.Left && !LudumCartridge.Cutscene.IsPlaying())
        {
            if (state == ButtonState.Pressed)
            {
                if (_inventory.IsGrabbed(this))
                {
                    _inventory.ClearGrabbedCard();
                }
                else
                {
                    _inventory.Grab(this);
                }
            }
        }
    }
}