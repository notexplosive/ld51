using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Input;
using MachinaLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class LudumCartridge : MachinaCartridge
{
    private bool _gameStarted;
    private static Scene GameScene { get; set; }
    private static Scene UiScene { get; set; }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1600, 900));

    public static Ui Ui { get; private set; }

    public static World World { get; private set; }

    public static Cutscene Cutscene { get; private set; }

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override IEnumerable<LoadEvent?> LoadEvents(Painter painter)
    {
        yield return
            new LoadEvent("Tiles", () => new GridBasedSpriteSheet("tiles", new Point(64)));

        yield return
            new LoadEvent("Plants", () => new GridBasedSpriteSheet("plant", new Point(128)));

        yield return
            new LoadEvent("Tools", () => new GridBasedSpriteSheet("tools", new Point(64)));

        yield return new LoadEvent("UI-Patch", () =>
        {
            var texture = Client.Assets.GetTexture("ui-bg");
            return new NinepatchSheet(texture, texture.Bounds, new Rectangle(new Point(3), new Point(58)));
        });

        yield return new LoadEvent("Card-Patch", () =>
        {
            var texture = Client.Assets.GetTexture("card");
            return new NinepatchSheet(texture, new Rectangle(Point.Zero, new Point(64)),
                new Rectangle(new Point(7), new Point(50)));
        });

        yield return new LoadEvent("Card-Back", () =>
        {
            var texture = Client.Assets.GetTexture("card");
            return new NinepatchSheet(texture, new Rectangle(new Point(64, 0), new Point(64)),
                new Rectangle(new Point(64 + 10, 10), new Point(44)));
        });

        yield return new LoadEvent("Tooltip", () =>
        {
            var texture = Client.Assets.GetTexture("tooltip");
            return new NinepatchSheet(texture, new Rectangle(Point.Zero, new Point(64)),
                new Rectangle(new Point(7), new Point(50)));
        });

        yield return new LoadEvent("Water", () => new GridBasedSpriteSheet("water", new Point(64)));
    }

    public override void OnCartridgeStarted()
    {
#if !DEBUG
        var mainMenu = AddSceneAsLayer();
        new MainMenu(mainMenu.AddActor("MainMenu"), BuildGameplay);
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

        LudumCartridge.World = new World(LudumCartridge.GameScene);
        LudumCartridge.Ui = new Ui(LudumCartridge.UiScene);
        LudumCartridge.Cutscene = new Cutscene(AddSceneAsLayer());

        new DebugComponent(LudumCartridge.GameScene.AddActor("debug"));

        var ui = LudumCartridge.Ui;
        var tutorial = ui.Tutorial;

        var world = LudumCartridge.World;
        tutorial.TellPlayerToClickOn(world.Tiles.GetRectangleAt(new Point(12, 5)),
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
                world.Tiles.SetContentAt(new Point(10, 5), TileContent.Dirt);
                world.Tiles.SetContentAt(new Point(14, 5), TileContent.Dirt);
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
            ()=> LudumCartridge.Cutscene.IsPlaying(),
            () =>
            {
                Fx.PutCardInDiscard(Fx.UiSpaceToGameSpace(ui.ReshuffleButtonBox.Rectangle.Center.ToVector2()), CropTemplate.Carrot);
                Fx.PutCardInDiscard(Fx.UiSpaceToGameSpace(ui.ReshuffleButtonBox.Rectangle.Center.ToVector2()), CropTemplate.Carrot);
                tutorial.Clear();
            });

        // eventually this will happen in the cutscene
        PlayerStats.Energy.Gain(80);

        for (var i = 0; i < 5; i++)
        {
            ui.Deck.AddCard(CropTemplate.Potato);
        }

        ui.Inventory.DrawNextCard(true);
    }

    public override void BeforeUpdate(float dt)
    {
        LudumCartridge.Ui?.Tooltip?.Clear();

        if (Client.Input.Keyboard.GetButton(Keys.F4).WasPressed && Client.Input.Keyboard.Modifiers.None)
        {
            Client.Window.SetFullscreen(!Client.Window.IsFullscreen);
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
        if (Client.Debug.IsPassiveOrActive)
        {
            if (key == Keys.A)
            {
            }
        }
    }
}
