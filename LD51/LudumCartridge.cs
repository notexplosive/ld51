using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD51;

public class LudumCartridge : MachinaCartridge
{
    private SeedInventory _inventory;
    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1600, 900));

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override IEnumerable<LoadEvent?> LoadEvents(Painter painter)
    {
        yield return
            new LoadEvent("Tiles", () =>
            {
                var texture = Client.Assets.GetTexture("tiles");
                return new GridBasedSpriteSheet(texture, new Point(64));
            });

        yield return
            new LoadEvent("Plants", () =>
            {
                var texture = Client.Assets.GetTexture("plant");
                return new GridBasedSpriteSheet(texture, new Point(128));
            });

        yield return
            new LoadEvent("Tools", () =>
            {
                var texture = Client.Assets.GetTexture("tools");
                return new GridBasedSpriteSheet(texture, new Point(64));
            });

        yield return new LoadEvent("UI-Patch", () =>
        {
            var texture = Client.Assets.GetTexture("ui-bg");
            return new NinepatchSheet(texture, texture.Bounds, new Rectangle(new Point(3), new Point(58)));
        });
    }

    public override void OnCartridgeStarted()
    {
        A.TileSheet = Client.Assets.GetAsset<SpriteSheet>("Tiles");
        A.TileRect = A.TileSheet.GetSourceRectForFrame(0);

        BuildGameScene();
        BuildUiScene();
    }

    private void BuildUiScene()
    {
        var uiScene = AddSceneAsLayer();
        var totalScreenSize = Client.Window.RenderResolution;
        var sheet = Client.Assets.GetAsset<NinepatchSheet>("UI-Patch");

        var resourcesActor = uiScene.AddActor("Resources");
        var resourcesBox = new Box(resourcesActor, new Point(totalScreenSize.X, 50));
        new NinepatchRenderer(resourcesActor, sheet);
        new Hoverable(resourcesActor);

        var energyText = resourcesActor.Transform.AddActorAsChild("EnergyText");
        new Box(energyText, new Point(200, resourcesBox.Rectangle.Height));
        new ResourceText(energyText);

        var inventoryActor = uiScene.AddActor("Inventory");
        inventoryActor.Transform.Position = new Vector2(0, totalScreenSize.Y - A.CardSize.Y * 2f / 3);

        var inventoryBackground = inventoryActor.Transform.AddActorAsChild("Background");
        inventoryBackground.Transform.LocalPosition += new Vector2(0, 32);
        new Box(inventoryBackground, new Point(totalScreenSize.X, A.CardSize.Y));
        new NinepatchRenderer(inventoryBackground, sheet);
        new Hoverable(inventoryBackground);

        _inventory = new SeedInventory(inventoryActor);
        _inventory.AddCard();
        _inventory.AddCard();
        _inventory.AddCard();
    }

    private void BuildGameScene()
    {
        var gameScene = AddSceneAsLayer();

        var gardenActor = gameScene.AddActor("Tiles");
        var tiles = new Tiles(gardenActor, new Point(25));
        new TileRenderer(gardenActor);
        new SelectedTileRenderer(gardenActor);
        var garden = new Garden(gardenActor, tiles);
        new GardenRenderer(gardenActor);
        gardenActor.Transform.Depth += 500;

        var guy = gameScene.AddActor("Guy");
        var box = new Box(guy, new Point(25, 50));
        box.Offset = new Point(box.Size.X / 2, box.Size.Y);
        new BoxRenderer(guy);
        var farmer = new Farmer(guy, tiles);

        tiles.TileTapped += position =>
        {
            if (farmer.InputBlocked)
            {
                return;
            }

            var farmerIsStandingOnTappedTile = farmer.CurrentTile.HasValue && farmer.CurrentTile.Value == position;

            if (garden.HasCropAt(position) && garden.GetCropAt(position).IsReadyToHarvest)
            {
                var crop = garden.GetCropAt(position);

                farmer.ClearTween();

                if (!farmerIsStandingOnTappedTile)
                {
                    farmer.EnqueueGoToTile(position);
                }

                farmer.EnqueueHarvestCrop(crop);
            }
            else if (_inventory.HasGrabbedCard())
            {
                var canPlantHere = tiles.GetContentAt(position).IsWet && garden.IsEmpty(position);
                if (canPlantHere)
                {
                    var crop = _inventory.GrabbedCard.Crop;
                    _inventory.Discard(_inventory.GrabbedCard);
                    _inventory.ClearGrabbedCard();

                    farmer.ClearTween();
                    if (!farmerIsStandingOnTappedTile)
                    {
                        farmer.EnqueueGoToTile(position, true);
                        farmer.EnqueueStepOffTile();
                    }

                    farmer.EnqueuePlantCrop(crop, garden, position);
                }
                else
                {
                    _inventory.ClearGrabbedCard();
                }
            }
            else
            {
                farmer.ClearTween();

                if (!farmerIsStandingOnTappedTile)
                {
                    _inventory.ClearGrabbedCard();
                    farmer.EnqueueGoToTile(position);
                }

                farmer.EnqueueUpgradeCurrentTile();
            }
        };

        guy.Transform.Position = new Vector2(500, 500);
    }
}

public class ResourceText : BaseComponent
{
    private readonly Box _box;
    private readonly Font _font;
    private readonly Texture2D _icon;

    public ResourceText(Actor actor) : base(actor)
    {
        _box = RequireComponent<Box>();
        _font = Client.Assets.GetFont("GameFont", 32);
        _icon = Client.Assets.GetTexture("energy");
    }

    public override void Draw(Painter painter)
    {
        var rectangle = _box.Rectangle;
        var iconPos = rectangle.Location.ToVector2() +
                      new Vector2(_icon.Width / 2f, rectangle.Height / 2f - _icon.Height / 2f);
        rectangle.Inflate(-_icon.Width, 0);
        rectangle.Location += new Point(_icon.Width, 0);
        painter.DrawAtPosition(_icon, iconPos);
        painter.DrawStringWithinRectangle(_font, "100", rectangle, Alignment.BottomLeft, new DrawSettings());
    }
}
