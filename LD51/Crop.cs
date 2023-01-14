using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExTween;
using Microsoft.Xna.Framework;

namespace LD51;

public delegate void CropEvent(CropEventData dataA, CropEventData dataB);

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

    public void Draw(Painter painter, Vector2 renderPos, Depth depth)
    {
        Client.Assets.GetAsset<SpriteSheet>("Plants").DrawFrameAtPosition(painter,
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

        if (Template.GrowCondition.CanGrow(_data, _timeAtCurrentLevel))
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

        AnimateBob();

        if (CurrentFrameOffset < Template.EffectiveMaxLevel)
        {
            _tiles.SetContentAt(_position, _tiles.GetContentAt(_position).Drain());
            CurrentFrameOffset++;

            Template.CropBehaviors.Grow.Run(_data, _data);

            if (IsReadyToHarvest)
            {
                Template.CropBehaviors.FinishedGrow.Run(_data, _data);
                Client.SoundPlayer.Play("tag2", new SoundEffectOptions());
            }
            else
            {
                Client.SoundPlayer.Play("tag", new SoundEffectOptions());
            }
        }
    }

    public void AnimateBob()
    {
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
    }

    public void Harvest(bool skipRemove = false)
    {
        if (!skipRemove)
        {
            _garden.RemoveCropAt(_position);
            Client.SoundPlayer.Play("brush", new SoundEffectOptions());
        }

        Template.CropBehaviors.Harvested.Run(_data, _data);
        
        foreach (var other in GetAdjacentCrops())
        {
            other.Template.CropBehaviors.HarvestAdjacent.Run(other._data,_data);
        }
    }

    public string ReportProgress()
    {
        return !IsReadyToHarvest ? Template.GrowCondition.Progress(_timeAtCurrentLevel) : "";
    }

    public void Plant()
    {
        Template.CropBehaviors.Planted.Run(_data, _data);

        foreach (var other in GetAdjacentCrops())
        {
            other.Template.CropBehaviors.PlantedAdjacent.Run(other._data,_data);
        }
    }

    public IEnumerable<Crop> GetAdjacentCrops()
    {
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                var other = _data.Garden.TryGetCropAt(_data.Position.GridPosition + new Point(x, y));
                if (other != null)
                {
                    yield return other;
                }
            }
        }
    }
}

public readonly record struct CropEventData(TilePosition Position, Garden Garden, CropTemplate Template, Tiles Tiles);
