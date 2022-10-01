using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class LudumCartridge : MachinaCartridge
{
    public static Scene GameScene { get; private set; }
    public static Scene UiScene { get; private set; }
    public SeedInventory Inventory { get; private set; }

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

        LudumCartridge.GameScene = AddSceneAsLayer();
        LudumCartridge.UiScene = AddSceneAsLayer();

        Fx.Setup(LudumCartridge.GameScene, LudumCartridge.UiScene);

        BuildGameScene();
        BuildUiScene();

        new DebugComponent(LudumCartridge.GameScene.AddActor("dbug"));
    }

    private void BuildUiScene()
    {
        var totalScreenSize = Client.Window.RenderResolution;
        var sheet = Client.Assets.GetAsset<NinepatchSheet>("UI-Patch");

        var resourcesActor = LudumCartridge.UiScene.AddActor("Resources");
        var resourcesBox = new Box(resourcesActor, new Point(totalScreenSize.X, 50));
        new NinepatchRenderer(resourcesActor, sheet);
        new Hoverable(resourcesActor);

        var energyText = resourcesActor.Transform.AddActorAsChild("EnergyText");
        new Box(energyText, new Point(200, resourcesBox.Rectangle.Height));
        new ResourceText(energyText, "Energy", PlayerStats.Energy);

        var inventoryActor = LudumCartridge.UiScene.AddActor("Inventory");
        inventoryActor.Transform.Position = new Vector2(0, totalScreenSize.Y - A.CardSize.Y * 2f / 3);

        Inventory = new SeedInventory(inventoryActor);
        Inventory.AddCard(CropTemplate.Potato);
        Inventory.AddCard(CropTemplate.Potato);
        Inventory.AddCard(CropTemplate.Potato);

        var deckActor = inventoryActor.Transform.AddActorAsChild("Deck");
        deckActor.Transform.LocalPosition += new Vector2(totalScreenSize.X - A.CardSize.X - 32, 0);
        deckActor.Transform.LocalDepth -= 10;
        new Box(deckActor, A.CardSize);
        var deck = new Deck(deckActor);
        new Hoverable(deckActor);
        new BoxRenderer(deckActor);
        new TextInBox(deckActor, A.CardTextFont, $"Draw Card\n({A.DrawCardCost} Energy)");
        var click = new Clickable(deckActor);
        click.Clicked += button =>
        {
            if (button == MouseButton.Left)
            {
                if (deck.IsNotEmpty() && PlayerStats.Energy.CanAfford(A.DrawCardCost))
                {
                    PlayerStats.Energy.Consume(A.DrawCardCost);
                    Inventory.AddCard(deck.DrawCard());
                }
            }
        };

        var inventoryBackground = inventoryActor.Transform.AddActorAsChild("Background");
        inventoryBackground.Transform.LocalPosition += new Vector2(0, 32);
        new Box(inventoryBackground, new Point(totalScreenSize.X, A.CardSize.Y));
        new NinepatchRenderer(inventoryBackground, sheet);
        new Hoverable(inventoryBackground);
    }

    private void BuildGameScene()
    {
        var gardenActor = LudumCartridge.GameScene.AddActor("Tiles");
        var tiles = new Tiles(gardenActor, new Point(25));
        new TileRenderer(gardenActor);
        new SelectedTileRenderer(gardenActor);
        var garden = new Garden(gardenActor, tiles);
        new GardenRenderer(gardenActor);
        gardenActor.Transform.Depth += 500;

        var guy = LudumCartridge.GameScene.AddActor("Guy");
        var texture = Client.Assets.GetTexture("scarecrow");
        new TextureRenderer(guy, texture, new DrawOrigin(new Vector2(texture.Width / 2f, texture.Height)));
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
            else if (Inventory.HasGrabbedCard())
            {
                var canPlantHere = tiles.GetContentAt(position).IsWet && garden.IsEmpty(position);
                if (canPlantHere)
                {
                    var crop = Inventory.GrabbedCard.Crop;
                    Inventory.Discard(Inventory.GrabbedCard);
                    Inventory.ClearGrabbedCard();

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
                    Inventory.ClearGrabbedCard();
                }
            }
            else
            {
                farmer.ClearTween();

                if (!farmerIsStandingOnTappedTile)
                {
                    Inventory.ClearGrabbedCard();
                    farmer.EnqueueGoToTile(position);
                }

                farmer.EnqueueUpgradeCurrentTile();
            }
        };

        guy.Transform.Position = new Vector2(500, 500);
    }
}

public class DebugComponent : BaseComponent
{
    public DebugComponent(Actor actor) : base(actor)
    {
    }

    public override void OnKey(Keys key, ButtonState state, ModifierKeys modifiers)
    {
        if (Client.Debug.IsPassiveOrActive)
        {
            if (key == Keys.A)
            {
            }
        }
    }
}
