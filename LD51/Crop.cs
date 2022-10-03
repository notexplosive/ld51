using System;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace LD51;

public delegate void CropEvent(CropEventData data);

public class Crop
{
    private readonly CropEventData _data;
    private readonly Garden _garden;
    private readonly TilePosition _position;
    private readonly Tiles _tiles;
    private readonly SequenceTween _tween = new();
    private Vector2 _scale = Vector2.One;
    private float _timeAtCurrentLevel;
    private float _totalTime;

    public Crop(CropEventData data)
    {
        _data = data;
        _garden = data.Garden;
        Template = data.Template;
        _tiles = data.Tiles;
        _position = data.Position;
    }

    public CropTemplate Template { get; }
    public int CurrentFrameOffset { get; private set; }
    public int Level => CurrentFrameOffset;

    public bool IsReadyToHarvest => CurrentFrameOffset == Template.EffectiveMaxLevel;
    public int PercentInCurrentLevel => (int)(_timeAtCurrentLevel / Template.GrowCondition.TickLength * 100);

    public void Draw(Painter painter, Vector2 renderPos, Depth depth)
    {
        Client.Assets.GetAsset<SpriteSheet>("Plants").DrawFrame(painter,
            Template.CropGraphic.FirstFrame + CurrentFrameOffset,
            renderPos, new Scale2D(_scale),
            new DrawSettings
                {Color = Color.White, Depth = depth, Origin = DrawOrigin.Center, Angle = MathF.Sin(_totalTime) / 16f});
    }

    public void Update(float dt)
    {
        if (_data.Tiles.GetContentAt(_data.Position).IsWet)
        {
            _totalTime += dt;
        }

        if (_tiles.GetContentAt(_position).IsWet)
        {
            _timeAtCurrentLevel += dt;
        }

        if (_timeAtCurrentLevel > Template.GrowCondition.TickLength && Template.GrowCondition.CanGrow(_data))
        {
            Grow();
        }

        _tween.Update(dt);
    }

    public void Grow(bool keepTimeAtCurrentLevel = false)
    {
        if (!keepTimeAtCurrentLevel)
        {
            _timeAtCurrentLevel = 0;
        }

        var scaleX = new TweenableFloat(() => _scale.X, x => _scale = new Vector2(x, _scale.Y));
        var scaleY = new TweenableFloat(() => _scale.Y, y => _scale = new Vector2(_scale.X, y));

        _tween.Clear();
        _tween.Add(
                new MultiplexTween()
                    .AddChannel(new Tween<float>(scaleX, 0.75f, 0.15f, Ease.QuadFastSlow))
                    .AddChannel(new Tween<float>(scaleY, 1.25f, 0.15f, Ease.QuadFastSlow))
            )
            .Add(
                new MultiplexTween()
                    .AddChannel(new Tween<float>(scaleX, 1.25f, 0.15f, Ease.QuadSlowFast))
                    .AddChannel(new Tween<float>(scaleY, 0.75f, 0.15f, Ease.QuadSlowFast))
            )
            .Add(
                new MultiplexTween()
                    .AddChannel(new Tween<float>(scaleX, 1f, 0.1f, Ease.QuadFastSlow))
                    .AddChannel(new Tween<float>(scaleY, 1f, 0.1f, Ease.QuadFastSlow))
            )
            ;

        if (CurrentFrameOffset < Template.EffectiveMaxLevel)
        {
            _tiles.SetContentAt(_position, _tiles.GetContentAt(_position).Drain());
            CurrentFrameOffset++;

            Grew?.Invoke(_data);

            if (IsReadyToHarvest)
            {
                FinishedGrowing?.Invoke(_data);
            }
        }
    }

    public event CropEvent Grew;
    public event CropEvent FinishedGrowing;
    public event CropEvent Harvested;

    public void Harvest(bool skipRemove = false)
    {
        if (!skipRemove)
        {
            _garden.RemoveCropAt(_position);
        }

        Harvested?.Invoke(_data);
    }
}

public readonly record struct CropEventData(TilePosition Position, Garden Garden, CropTemplate Template, Tiles Tiles);
