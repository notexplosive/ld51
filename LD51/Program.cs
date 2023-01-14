using ExplogineDesktop;
using ExplogineMonoGame;
using LD51;
using Microsoft.Xna.Framework;

var config = new WindowConfigWritable
{
    WindowSize = new Point(1600, 900),
    Title = "Harvest Golem - NotExplosive.net",
#if !DEBUG
    Fullscreen = true
#endif
};
Bootstrap.Run(args, new WindowConfig(config), runtime => new LudumCartridge(runtime), "--skipSnapShot");
