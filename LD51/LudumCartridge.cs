﻿using System.Collections.Generic;
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
    private static Scene GameScene { get; set; }
    private static Scene UiScene { get; set; }

    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1600, 900));

    public static Ui Ui { get; set; }

    public static World World { get; set; }

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
                new Rectangle(new Point(64 + 7, 7), new Point(50)));
        });
    }

    public override void OnCartridgeStarted()
    {
        A.TileSheet = Client.Assets.GetAsset<SpriteSheet>("Tiles");
        A.TileRect = A.TileSheet.GetSourceRectForFrame(0);

        LudumCartridge.GameScene = AddSceneAsLayer();
        LudumCartridge.UiScene = AddSceneAsLayer();

        LudumCartridge.World = new World(LudumCartridge.GameScene);
        LudumCartridge.Ui = new Ui(LudumCartridge.UiScene);

        new DebugComponent(LudumCartridge.GameScene.AddActor("dbug"));
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
