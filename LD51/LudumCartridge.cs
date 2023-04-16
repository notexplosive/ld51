using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using MachinaLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class LudumCartridge : MachinaCartridge
{
    private bool _gameStarted;

    public LudumCartridge(IRuntime runtime) : base(runtime)
    {
    }

    private static Scene GameScene { get; set; }
    private static Scene UiScene { get; set; }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1920, 1080));

    public static Ui Ui { get; private set; }

    public static World World { get; private set; }

    public static Cutscene Cutscene { get; private set; }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override IEnumerable<ILoadEvent?> LoadEvents(Painter painter)
    {
        yield return
            new AssetLoadEvent("Tiles", () => new GridBasedSpriteSheet("tiles", new Point(64)));

        yield return
            new AssetLoadEvent("Plants", () => new GridBasedSpriteSheet("plant", new Point(128)));

        yield return
            new AssetLoadEvent("Tools", () => new GridBasedSpriteSheet("tools", new Point(64)));

        yield return new AssetLoadEvent("UI-Patch", () =>
        {
            var texture = Client.Assets.GetTexture("ui-bg");
            return new NinepatchSheet(texture, texture.Bounds, new Rectangle(new Point(3), new Point(58)));
        });

        yield return new AssetLoadEvent("Card-Patch", () =>
        {
            var texture = Client.Assets.GetTexture("card");
            return new NinepatchSheet(texture, new Rectangle(Point.Zero, new Point(64)),
                new Rectangle(new Point(7), new Point(50)));
        });

        yield return new AssetLoadEvent("Card-Back", () =>
        {
            var texture = Client.Assets.GetTexture("card");
            return new NinepatchSheet(texture, new Rectangle(new Point(64, 0), new Point(64)),
                new Rectangle(new Point(64 + 10, 10), new Point(44)));
        });

        yield return new AssetLoadEvent("Tooltip", () =>
        {
            var texture = Client.Assets.GetTexture("tooltip");
            return new NinepatchSheet(texture, new Rectangle(Point.Zero, new Point(64)),
                new Rectangle(new Point(7), new Point(50)));
        });

        yield return new AssetLoadEvent("Water", () => new GridBasedSpriteSheet("water", new Point(64)));

        yield return new AssetLoadEvent("MenuBackground", () =>
        {
            var canvas = new Canvas(512, 512);
            var tiles = Client.Assets.GetAsset<SpriteSheet>("Tiles");

            Client.Graphics.PushCanvas(canvas);
            Client.Graphics.Painter.BeginSpriteBatch(Matrix.Identity);
            var ints = new[] {0, 2, 3};
            for (var x = 0; x < canvas.Size.X; x += 64)
            {
                for (var y = 0; y < canvas.Size.Y; y += 64)
                {
                    tiles.DrawFrameAtPosition(Client.Graphics.Painter, Client.Random.Dirty.GetRandomElement(ints),
                        new Vector2(x, y), Scale2D.One, new DrawSettings());
                }
            }

            Client.Graphics.Painter.EndSpriteBatch();
            Client.Graphics.PopCanvas();

            return canvas.AsTextureAsset();
        });
    }
    
    public override void OnCartridgeStarted()
    {
#if !DEBUG
        var mainMenu = AddSceneAsLayer();
        new MainMenu(mainMenu.AddActor("MainMenu"), BuildGameplay, Runtime);
#else
        BuildGameplay();
#endif
    }

    private void BuildGameplay()
    {
        _gameStarted = true;

        A.TileSheet = Client.Assets.GetAsset<SpriteSheet>("Tiles");
        A.TileRect = A.TileSheet.GetSourceRectForFrame(0);

        LudumCartridge.GameScene = AddSceneAsLayer();
        LudumCartridge.UiScene = AddSceneAsLayer();

        LudumCartridge.World = new World(LudumCartridge.GameScene, Runtime);
        LudumCartridge.Ui = new Ui(LudumCartridge.UiScene, Runtime);
        LudumCartridge.Cutscene = new Cutscene(AddSceneAsLayer(), Runtime);

        new DebugComponent(LudumCartridge.GameScene.AddActor("debug"));

        var ui = LudumCartridge.Ui;
        var tutorial = ui.Tutorial;

        var world = LudumCartridge.World;
        world.Tiles.SetContentAt(new Point(14, 5), TileContent.Dirt);

        tutorial.TellPlayerToClickOn(world.Tiles.GetRectangleAt(new Point(14, 5)),
            "Click On Dirt to Till");
        tutorial.AddTrigger(
            () => world.Tiles.HasAnyContent(TileContent.Tilled),
            () =>
            {
                var location = world.Tiles.GetATileWithContent(TileContent.Tilled);
                if (location.HasValue)
                {
                    tutorial.TellPlayerToClickOn(
                        world.Tiles.GetRectangleAt(location.Value), "Click on Tilled Soil to water it");
                }
            });
        tutorial.AddTrigger(
            () => world.Tiles.HasAnyContent(TileContent.WateredL3),
            () =>
            {
                tutorial.TellPlayerToClickOn(
                    ui.Inventory.GetCard(0).Rectangle, "Select a Seed");
            });

        tutorial.AddTrigger(
            () => ui.Inventory.HasGrabbedCard(),
            () =>
            {
                var location = world.Tiles.GetATileWithContent(TileContent.WateredL3);
                if (location.HasValue)
                {
                    tutorial.TellPlayerToClickOn(
                        world.Tiles.GetRectangleAt(location.Value), "Plant it!");
                }
            });

        tutorial.AddTrigger(
            () => world.Garden.HasAnyCrop(),
            () =>
            {
                tutorial.Clear();
                world.Tiles.SetContentAt(new Point(13, 5), TileContent.Dirt);
                world.Tiles.SetContentAt(new Point(15, 5), TileContent.Dirt);
            });

        tutorial.AddTrigger(
            () => !world.Garden.HasAnyCrop()
                  || !(world.Tiles.HasAnyContent(TileContent.Tilled) ||
                       world.Tiles.HasAnyContent(TileContent.Dirt)),
            () =>
            {
                tutorial.TellPlayerToClickOn(
                    ui.Deck.Rectangle, "Draw more Seeds");
            });

        tutorial.AddTrigger(
            () => ui.Inventory.Count > 0,
            () => { tutorial.Clear(); });

        tutorial.AddTrigger(
            () => world.IsSoftLocked(),
            () =>
            {
                tutorial.TellPlayerToClickOn(
                    ui.ReshuffleButtonBox.Rectangle, "Nothing else you can do...");
            });

        tutorial.AddTrigger(
            () => LudumCartridge.Cutscene.IsPlaying(),
            () => { tutorial.Clear(); });

        // eventually this will happen in the cutscene
        PlayerStats.Energy.Gain(80);

        for (var i = 0; i < 5; i++)
        {
            ui.Deck.AddCard(CropTemplate.GetByName("Collard"));
        }

        ui.Inventory.DrawNextCard(true);

        var skip = true;
#if !DEBUG
        skip = false;
#endif
        LudumCartridge.Cutscene.PlayOpening(skip);
    }

    public override void BeforeUpdate(float dt)
    {
        LudumCartridge.Ui?.Tooltip?.Clear();

        if (Client.Input.Keyboard.GetButton(Keys.F4).WasPressed && Client.Input.Keyboard.Modifiers.None)
        {
            Runtime.Window.SetFullscreen(!Runtime.Window.IsFullscreen);
        }
    }

    public override void AfterUpdate(float dt)
    {
        if (_gameStarted)
        {
            // fx tween cleanup
            if (Fx.EventTween.IsDone())
            {
                Fx.EventTween.Clear();
            }
            else
            {
                Fx.EventTween.Update(dt);
            }

            LudumCartridge.Cutscene.Tween.Update(dt);
        }
    }
}

public class DebugComponent : BaseComponent
{
    public DebugComponent(Actor actor) : base(actor)
    {
    }

    public override void OnKey(Keys key, ButtonState state, ModifierKeys modifiers)
    {
        if (Client.Debug.IsPassiveOrActive && state == ButtonState.Pressed)
        {
            if (key == Keys.W)
            {
                LudumCartridge.Ui.Inventory.AddCard(CropTemplate.GetByName("Pumpkin"));
            }

            if (key == Keys.E)
            {
                foreach (var template in CropTemplate.AllTemplates)
                {
                    LudumCartridge.Ui.Deck.AddCard(template);
                }
            }

            if (key == Keys.R)
            {
                PlayerStats.Energy.Gain(500);
            }

            if (key == Keys.T)
            {
                Client.Debug.Log(CropTemplate.GetRandomOfRarity(Rarity.Rare).Name);
            }

            if (key == Keys.Y)
            {
                Client.Debug.Log(CropTemplate.GetRandomOfRarity(Rarity.Common).Name);
            }
        }
    }
}
