using System;
using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.HitTesting;
using ExplogineMonoGame.Input;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class LudumCartridge : MachinaCartridge
{
    private Scene _scene;
    public override CartridgeConfig CartridgeConfig { get; } = new(new Point(1600, 900));

    public override void AddCommandLineParameters(CommandLineParametersWriter parameters)
    {
    }

    public override IEnumerable<LoadEvent?> LoadEvents(Painter painter)
    {
        yield return null;
    }
    
    public override void OnCartridgeStarted()
    {
        _scene = AddSceneAsLayer();
        var guy = _scene.AddActor("Guy");
        new Box(guy, new Point(50));
        new BoxRenderer(guy);
        new Hoverable(guy);

        guy.Transform.Position = new Vector2(500, 500);
    }
}
