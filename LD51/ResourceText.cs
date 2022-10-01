using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51;

public class ResourceText : BaseComponent
{
    private readonly Box _box;
    private readonly Font _font;
    private readonly Texture2D _icon;
    private readonly string _resourceName;
    private readonly PlayerStats.Stat _stat;

    public ResourceText(Actor actor, string resourceName, PlayerStats.Stat stat) : base(actor)
    {
        _box = RequireComponent<Box>();
        _font = Client.Assets.GetFont("GameFont", 32);
        _icon = Client.Assets.GetTexture("energy");
        _resourceName = resourceName;
        _stat = stat;
    }

    public override void Draw(Painter painter)
    {
        var rectangle = _box.Rectangle;
        var iconPos = rectangle.Location.ToVector2() +
                      new Vector2(_icon.Width / 2f, rectangle.Height / 2f - _icon.Height / 2f);
        rectangle.Inflate(-_icon.Width, 0);
        rectangle.Location += new Point(_icon.Width, 0);
        painter.DrawAtPosition(_icon, iconPos);
        painter.DrawStringWithinRectangle(_font, $"{_resourceName}: {_stat.Amount}", rectangle, Alignment.BottomLeft, new DrawSettings());
    }
}
