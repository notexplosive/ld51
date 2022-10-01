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

    public Crop(Garden garden, CropTemplate template, Tiles tiles, TilePosition position)
    {
        _garden = garden;
        _template = template;
        _tiles = tiles;
        _position = position;
    }

    public bool IsReadyToHarvest => _level == _template.EffectiveMaxLevel;

    public void Draw(Painter painter, Vector2 renderPos, Depth depth)
    {
        Client.Assets.GetAsset<SpriteSheet>("Plants").DrawFrame(painter, _template.CropLevel.FirstFrame + _level,
            renderPos, Scale2D.One,
            new DrawSettings
                {Color = Color.White, Depth = depth, Origin = DrawOrigin.Center, Angle = MathF.Sin(_totalTime) / 8f});
    }

    public void Update(float dt)
    {
        _totalTime += dt;

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

            Grew?.Invoke();

            if (IsReadyToHarvest)
            {
                FinishedGrowing?.Invoke();
            }
        }
    }

    public event Action Grew;
    public event Action FinishedGrowing;

    public void Harvest()
    {
        _garden.RemoveCropAt(_position);
    }
}
