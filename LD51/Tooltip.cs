using System;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class Tooltip : BaseComponent
{
    private readonly Font _textFont;
    private readonly Font _titleFont;

    public readonly record struct Content(string Title, string Text);

    public Tooltip(Actor actor) : base(actor)
    {
        _currentContent = new Content("Title","This is some test text, to see how much we can fit on the tooltip at once.");
        _textFont = A.TooltipTextFont;
        _titleFont = A.TooltipTitleFont;
    }

    private Content _currentContent;

    public override void Draw(Painter painter)
    {
        if (string.IsNullOrEmpty(_currentContent.Title))
        {
            return;
        }
        
        var sheet = Client.Assets.GetAsset<NinepatchSheet>("Tooltip");
        var padding = 10;


        var maxWidth = 600f;
        var titleBounds = _titleFont.MeasureString(_currentContent.Title);
        maxWidth = MathF.Max(titleBounds.X + 5, maxWidth);
        
        var maxBounds = _textFont.MeasureString(_currentContent.Text, maxWidth);
        maxBounds += new Vector2(0, _titleFont.FontSize);
        maxBounds.X = MathF.Max(maxWidth, maxBounds.X);

        var allTextRect = new Rectangle(Transform.Position.ToPoint(), maxBounds.ToPoint());

        var ninepatchRect = allTextRect;
        ninepatchRect.Inflate(padding, padding);
        sheet.DrawFullNinepatch(painter, ninepatchRect, InnerOuter.Inner, Transform.Depth);

        var titleRect =
            new Rectangle(Transform.Position.ToPoint(), new Point(allTextRect.Width, _titleFont.FontSize));

        var descriptionRect = new Rectangle(Transform.Position.ToPoint() + new Point(0, _titleFont.FontSize),
            new Point(allTextRect.Width, allTextRect.Height - _titleFont.FontSize));

        painter.DrawStringWithinRectangle(_titleFont, _currentContent.Title, titleRect, Alignment.CenterLeft,
            new DrawSettings {Color = Color.White});
        painter.DrawStringWithinRectangle(_textFont, _currentContent.Text, descriptionRect, Alignment.TopLeft,
            new DrawSettings {Color = Color.White});
    }

    public void Clear()
    {
        _currentContent = new Content();
    }
    
    public void Set(string title, string description)
    {
        _currentContent = new Content(title, description);
    }
}
