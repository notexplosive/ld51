using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

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

        var inventoryActor = uiScene.AddActor("Inventory");

        inventoryActor.Transform.Position = new Vector2(32, totalScreenSize.Y - A.CardSize.Y * 2f / 3);

        _inventory = new SeedInventory(inventoryActor);
        _inventory.AddCard();
        _inventory.AddCard();
        _inventory.AddCard();
    }

    private void BuildGameScene()
    {
        var gameScene = AddSceneAsLayer();
        var guy = gameScene.AddActor("Guy");
        var box = new Box(guy, new Point(25, 50));
        box.Offset = new Point(box.Size.X / 2, box.Size.Y);
        new BoxRenderer(guy);
        var farmer = new Farmer(guy);

        var gardenActor = gameScene.AddActor("Tiles");
        var tiles = new Tiles(gardenActor, new Point(25));
        new TileRenderer(gardenActor);
        new SelectedTileRenderer(gardenActor);
        var garden = new Garden(gardenActor);
        new GardenRenderer(gardenActor);
        gardenActor.Transform.Depth += 500;

        tiles.TileTapped += position =>
        {
            if (farmer.CurrentTile.HasValue && farmer.CurrentTile.Value == position)
            {
                if (_inventory.HasGrabbedCard())
                {
                    if (tiles.GetContentAt(position) == TileContent.Watered && garden.IsEmpty(position))
                    {
                        garden.PlantSeed(_inventory.GrabbedSeed(), position);
                        _inventory.DiscardGrabbedCard();
                    }
                    
                    _inventory.ClearGrabbedCard();
                }
                else
                {
                    tiles.PutTileContentAt(position, tiles.GetContentAt(position).Upgrade());
                }
            }
            else
            {
                _inventory.ClearGrabbedCard();
                farmer.GoToTile(position);
            }
        };

        guy.Transform.Position = new Vector2(500, 500);
    }
}