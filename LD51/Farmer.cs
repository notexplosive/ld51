using System;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class Farmer : BaseComponent
{
    public enum Tool
    {
        None,
        Hoe,
        WateringCan
    }

    private readonly float _speed = 400f;
    private readonly Tiles _tiles;
    private readonly TweenableFloat _toolAngle = new(0);
    private readonly SequenceTween _tween = new();
    private readonly TweenableVector2 _tweenablePosition;
    private int _blockingFlags;
    private Tool _currentShownTool;
    private float _totalWalkTime;
    private bool _isWalking;

    public Farmer(Actor actor, Tiles tiles) : base(actor)
    {
        _tiles = tiles;
        _tweenablePosition = new TweenableVector2(() => Transform.Position, val => Transform.Position = val);
    }

    public TilePosition? CurrentTile { get; set; }
    public bool InputBlocked => _blockingFlags > 0;

    public override void Draw(Painter painter)
    {
        if (_currentShownTool != Tool.None)
        {
            var frameIndex = (int) _currentShownTool - 1;
            Client.Assets.GetAsset<SpriteSheet>("Tools").DrawFrame(painter, frameIndex,
                Transform.Position + new Vector2(0, -20), Scale2D.One,
                new DrawSettings
                    {Color = Color.White, Angle = _toolAngle, Origin = new DrawOrigin(new Vector2(13, 50))});
        }
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);

        if (_isWalking)
        {
            _totalWalkTime += dt;
            Transform.Angle = MathF.Sin(_totalWalkTime * 10) / 10;
        }
        else
        {
            _totalWalkTime = 0f;
            Transform.Angle = 0;
        }
    }

    public void ClearTween()
    {
        _blockingFlags = 0;
        _currentShownTool = Tool.None;
        _isWalking = false;
        _tween.Clear();
    }

    public ITween WalkTo(Vector2 target)
    {
        var distance = (_tweenablePosition.Value - target).Length();
        var duration = distance / _speed;

        if (distance == 0)
        {
            return new SequenceTween();
        }

        return
            new SequenceTween()
                .Add(new CallbackTween(()=>_isWalking = true))
                .Add(new Tween<Vector2>(_tweenablePosition, target, duration, Ease.Linear))
                .Add(new CallbackTween(()=>_isWalking = false))
            ;
    }

    public void EnqueueGoToTile(TilePosition tilePosition, bool blockInput = false)
    {
        CurrentTile = null;

        if (blockInput)
        {
            Enqueue(HaltInput());
        }
        
        Enqueue(WalkTo(tilePosition.Rectangle.Center.ToVector2() - new Vector2(A.TileRect.Width / 2f, 0)));
        Enqueue(new CallbackTween(() => { CurrentTile = tilePosition; }));
        
        if (blockInput)
        {
            Enqueue(RestoreInput());
        }
    }

    public void UpgradeCurrentTile()
    {
        if (CurrentTile != null)
        {
            _tiles.PutTileContentAt(CurrentTile.Value, _tiles.GetContentAt(CurrentTile.Value).Upgrade());
        }
    }

    private void Enqueue(ITween tween)
    {
        _tween.Add(tween);
    }

    private ITween RestoreInput()
    {
        return new CallbackTween(() => _blockingFlags--);
    }

    private ITween HaltInput()
    {
        return new CallbackTween(() => _blockingFlags++);
    }

    public void EnqueueUpgradeCurrentTile()
    {
        Enqueue(UpgradeCurrentTileTween());
    }

    private ITween UpgradeCurrentTileTween()
    {
        var result = new SequenceTween();
        result.Add(HaltInput());
        result.Add(new DynamicTween(() =>
        {
            var dynamicResult = new SequenceTween();

            if (CurrentTile.HasValue)
            {
                var tileContent = _tiles.GetContentAt(CurrentTile.Value);
                if (tileContent == TileContent.Dirt)
                {
                    dynamicResult.Add(new CallbackTween(() => _toolAngle.Value = 0f));
                    dynamicResult.Add(ShowTool(Tool.Hoe));
                    dynamicResult.Add(new Tween<float>(_toolAngle, -MathF.PI / 2f, 0.5f, Ease.QuadFastSlow));
                    dynamicResult.Add(new Tween<float>(_toolAngle, MathF.PI / 4f, 0.25f, Ease.QuadSlowFast));
                    dynamicResult.Add(new CallbackTween(UpgradeCurrentTile));
                    dynamicResult.Add(new WaitSecondsTween(0.1f));
                }

                if (tileContent == TileContent.Tilled || tileContent.IsWet)
                {
                    dynamicResult.Add(ShowTool(Tool.WateringCan));
                    dynamicResult.Add(new CallbackTween(() => _toolAngle.Value = 0f));
                    dynamicResult.Add(new Tween<float>(_toolAngle, MathF.PI / 4f, 0.25f, Ease.QuadFastSlow));
                    dynamicResult.Add(new WaitSecondsTween(0.1f));
                    dynamicResult.Add(new CallbackTween(UpgradeCurrentTile));
                    dynamicResult.Add(new WaitSecondsTween(0.1f));
                    dynamicResult.Add(new Tween<float>(_toolAngle, 0, 0.25f, Ease.QuadSlowFast));
                    dynamicResult.Add(new WaitSecondsTween(0.15f));
                }
            }

            return dynamicResult;
        }));

        result.Add(HideTool());
        result.Add(RestoreInput());

        return result;
    }

    private ITween HideTool()
    {
        return new CallbackTween(() => _currentShownTool = Tool.None);
    }

    private ITween ShowTool(Tool tool)
    {
        return new CallbackTween(() => _currentShownTool = tool);
    }

    public void EnqueuePlantCrop(CropEventData data)
    {
        Enqueue(new CallbackTween(() => data.Garden.PlantCrop(data)));
    }

    public void EnqueueHarvestCrop(Crop crop)
    {
        Enqueue(HaltInput());
        Enqueue(new WaitSecondsTween(0.25f));
        Enqueue(new CallbackTween(()=>crop.Harvest()));
        Enqueue(RestoreInput());
    }
}
