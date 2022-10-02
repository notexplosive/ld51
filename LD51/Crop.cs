using System;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using Microsoft.Xna.Framework;

namespace LD51;

public class Crop
{
    private readonly TilePosition _position;
    private readonly Garden _garden;
    private readonly CropTemplate _template;
    private readonly Tiles _tiles;
    private int _level;
    private float _timeAtCurrentLevel;
    private float _totalTime;
    private readonly CropEventData _data;

    public Crop(CropEventData data)
    {
        _data = data;
        _garden = data.Garden;
        _template = data.Template;
        _tiles = data.Tiles;
        _position = data.Position;
    }

    public bool IsReadyToHarvest => _level == _template.EffectiveMaxLevel;

    public void Draw(Painter painter, Vector2 renderPos, Depth depth)
    {
        Client.Assets.GetAsset<SpriteSheet>("Plants").DrawFrame(painter, _template.CropGraphic.FirstFrame + _level,
            renderPos, Scale2D.One,
            new DrawSettings
                {Color = Color.White, Depth = depth, Origin = DrawOrigin.Center, Angle = MathF.Sin(_totalTime) / 8f});
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
            _tiles.PutTileContentAt(_position, _tiles.GetContentAt(_position).Downgrade());
            _level++;

            Grew?.Invoke(_data);

            if (IsReadyToHarvest)
            {
                FinishedGrowing?.Invoke(_data);
            }
        }
    }

    public delegate void CropEvent(CropEventData data);
    
    public event CropEvent Grew;
    public event CropEvent FinishedGrowing;
    public event CropEvent Harvested;

    public void Harvest()
    {
        _garden.RemoveCropAt(_position);
        if (_template.IsRecyclable)
        {
            Fx.PutCardInDiscard(_data.Position.Rectangle.Center.ToVector2(), _data.Template);
        }

        Harvested?.Invoke(_data);
    }
}


public readonly record struct CropEventData(TilePosition Position, Garden Garden, CropTemplate Template, Tiles Tiles);
