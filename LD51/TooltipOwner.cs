using ExplogineMonoGame;
using ExplogineMonoGame.HitTesting;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class TooltipOwner : BaseComponent
{
    private readonly string _title;
    private readonly string _content;
    private readonly Hoverable _hoverable;

    public TooltipOwner(Actor actor, string title, string content) : base(actor)
    {
        _title = title;
        _content = content;
        _hoverable = RequireComponent<Hoverable>();
    }

    public override void Update(float dt)
    {
        if (_hoverable.IsHovered)
        {
            LudumCartridge.Ui.Tooltip.Set(_title, _content);
        }
    }
}
