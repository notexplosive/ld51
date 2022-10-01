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
    }

    public void ClearTween()
    {
        _blockingFlags = 0;
        _currentShownTool = Tool.None;
        _tween.Clear();
    }

    public void EnqueueWalkTo(Vector2 target)
    {
        var distance = (_tweenablePosition.Value - target).Length();
        var duration = distance / _speed;

        if (distance == 0)
        {
            return;
        }

        _tween.Add(new Tween<Vector2>(_tweenablePosition, target, duration, Ease.Linear));
    }

    public void EnqueueGoToTile(TilePosition tilePosition)
    {
        CurrentTile = null;
        EnqueueWalkTo(tilePosition.Rectangle.Center.ToVector2());
        _tween.Add(new CallbackTween(() => { CurrentTile = tilePosition; }));
    }

    public void UpgradeCurrentTile()
    {
        if (CurrentTile != null)
        {
            _tiles.PutTileContentAt(CurrentTile.Value, _tiles.GetContentAt(CurrentTile.Value).Upgrade());
        }
    }

    public void EnqueueStepOffTile()
    {
        EnqueueHaltInput();
        EnqueueWalkTo(CurrentTile.Value.Rectangle.Center.ToVector2() - new Vector2(A.TileRect.Width / 2f, 0));
        _tween.Add(new WaitSecondsTween(0.15f));
        EnqueueRestoreInput();
    }

    private void EnqueueRestoreInput()
    {
        _tween.Add(new CallbackTween(() => _blockingFlags--));
    }

    private void EnqueueHaltInput()
    {
        _tween.Add(new CallbackTween(() => _blockingFlags++));
    }

    public void EnqueueUpgradeCurrentTile()
    {
        EnqueueHaltInput();
        EnqueueStepOffTile();
        if (CurrentTile.HasValue)
        {
            var tileContent = _tiles.GetContentAt(CurrentTile.Value);
            if (tileContent == TileContent.Normal)
            {
                _toolAngle.Value = 0f;
                _tween.Add(ShowTool(Tool.Hoe));
                _tween.Add(new Tween<float>(_toolAngle, -MathF.PI / 2f, 0.5f, Ease.QuadFastSlow));
                _tween.Add(new Tween<float>(_toolAngle, MathF.PI / 4f, 0.25f, Ease.QuadSlowFast));
                _tween.Add(new CallbackTween(UpgradeCurrentTile));
                _tween.Add(new WaitSecondsTween(0.1f));
            }

            if (tileContent == TileContent.Tilled)
            {
                _tween.Add(ShowTool(Tool.WateringCan));
                _toolAngle.Value = 0f;
                _tween.Add(new Tween<float>(_toolAngle, MathF.PI / 4f, 0.25f, Ease.QuadFastSlow));
                _tween.Add(new WaitSecondsTween(0.1f));
                _tween.Add(new CallbackTween(UpgradeCurrentTile));
                _tween.Add(new WaitSecondsTween(0.1f));
                _tween.Add(new Tween<float>(_toolAngle, 0, 0.25f, Ease.QuadSlowFast));
                _tween.Add(new WaitSecondsTween(0.15f));
            }

            _tween.Add(HideTool());

            EnqueueRestoreInput();
        }
    }

    private ITween HideTool()
    {
        return new CallbackTween(() => _currentShownTool = Tool.None);
    }

    private ITween ShowTool(Tool tool)
    {
        return new CallbackTween(() => _currentShownTool = tool);
    }

    public void EnqueuePlantCrop(Crop crop, Garden garden, TilePosition position)
    {
        EnqueueStepOffTile();
        _tween.Add(new CallbackTween(() => garden.PlantCrop(crop, position)));
    }

    public void EnqueueDiscardGrabbedCard(SeedInventory inventory)
    {
        inventory.DiscardGrabbedCard();
        inventory.ClearGrabbedCard();
    }
}
