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
    private Scene _scene;
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
    }

    public override void OnCartridgeStarted()
    {
        A.TileSheet = Client.Assets.GetAsset<SpriteSheet>("Tiles");
        A.TileRect = A.TileSheet.GetSourceRectForFrame(0);

        _scene = AddSceneAsLayer();
        var guy = _scene.AddActor("Guy");
        var box = new Box(guy, new Point(25, 50));
        box.Offset = new Point(box.Size.X / 2, box.Size.Y);
        new BoxRenderer(guy);
        var farmer = new Farmer(guy);

        var tileActor = _scene.AddActor("Tiles");
        var tiles = new Tiles(tileActor, new Point(25));
        new TileRenderer(tileActor);
        new SelectedTileRenderer(tileActor);
        tileActor.Transform.Depth += 500;


        tiles.TileTapped += position =>
        {
            if (farmer.CurrentTile.HasValue && farmer.CurrentTile.Value == position)
            {
                tiles.PutTileContentAt(position, tiles.GetContentAt(position).Upgrade());
            }
            else
            {
                farmer.GoToTile(position);
            }
        };
        
        guy.Transform.Position = new Vector2(500, 500);
    }
}