using System;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD51;

public record CropLevel(int MaxLevel, int FirstFrame);

public record CropTemplate(int TickLength, CropLevel CropLevel)
{
    public static CropTemplate Potato = new(10, new CropLevel(3, 0));
    public int EffectiveMaxLevel => CropLevel.MaxLevel - 1;

    public Crop CreateCrop()
    {
        return new Crop(this);
    }
}

public class Crop
{
    private readonly CropTemplate _template;
    private int _level;
    private float _timeAtCurrentLevel;
    private float _totalTime;

    public Crop(CropTemplate template)
    {
        _template = template;
    }

    public void Draw(Painter painter, Vector2 renderPos, Depth depth)
    {
        Client.Assets.GetAsset<SpriteSheet>("Plants").DrawFrame(painter, _template.CropLevel.FirstFrame + _level,
            renderPos, Scale2D.One,
            new DrawSettings
                {Color = Color.White, Depth = depth, Origin = DrawOrigin.Center, Angle = MathF.Sin(_totalTime) / 4f});
    }

    public void Update(float dt)
    {
        _totalTime += dt;
        _timeAtCurrentLevel += dt;

        if (_timeAtCurrentLevel > _template.TickLength)
        {
            Grow();
        }
    }

    private void Grow()
    {
        _timeAtCurrentLevel = 0;

        if (_level < _template.EffectiveMaxLevel)
        {
            _level++;
            Grew?.Invoke();

            if (_level == _template.EffectiveMaxLevel)
            {
                FinishedGrowing?.Invoke();
            }
        }
    }

    public event Action Grew;
    public event Action FinishedGrowing;
}
