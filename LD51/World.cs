using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class World
{
    public World(Scene scene)
    {
        Scene = scene;
        var gardenActor = scene.AddActor("Tiles");
        Tiles = new Tiles(gardenActor, new Point(25));
        new TileRenderer(gardenActor);
        Garden = new Garden(gardenActor, Tiles);
        new GardenRenderer(gardenActor);
        gardenActor.Transform.Depth += 500;

        var guy = scene.AddActor("Guy");
        var texture = Client.Assets.GetTexture("scarecrow");
        new TextureRenderer(guy, texture, new DrawOrigin(new Vector2(texture.Width / 2f, texture.Height)));
        var farmer = new Farmer(guy, Tiles);

        new SelectedTileRenderer(gardenActor, farmer);

        Tiles.TileTapped += position =>
        {
            if (farmer.InputBlocked)
            {
                return;
            }
            farmer.ClearTween();
            
            var farmerIsStandingOnTappedTile = farmer.CurrentTile.HasValue && farmer.CurrentTile.Value == position;

            if (Garden.HasCropAt(position) && Garden.GetCropAt(position).IsReadyToHarvest)
            {
                var crop = Garden.GetCropAt(position);

                if (!farmerIsStandingOnTappedTile)
                {
                    farmer.EnqueueGoToTile(position);
                }

                farmer.EnqueueHarvestCrop(crop);
            }
            else if (LudumCartridge.Ui.Inventory.HasGrabbedCard())
            {
                var canPlantHere = Tiles.GetContentAt(position).IsWet && Garden.IsEmpty(position);
                if (canPlantHere)
                {
                    var template = LudumCartridge.Ui.Inventory.GrabbedCard.CropTemplate;
                    LudumCartridge.Ui.Inventory.Remove(LudumCartridge.Ui.Inventory.GrabbedCard);
                    LudumCartridge.Ui.Inventory.ClearGrabbedCard();

                    if (!farmerIsStandingOnTappedTile)
                    {
                        farmer.EnqueueGoToTile(position);
                    }

                    farmer.EnqueuePlantCrop(new CropEventData(position, Garden, template, Tiles));
                }
                else
                {
                    LudumCartridge.Ui.ErrorToast.ShowError("Cannot plant there");
                    LudumCartridge.Ui.Inventory.ClearGrabbedCard();
                }
            }
            else
            {

                var content = Tiles.GetContentAt(position);
                if (content.Upgrade() == content)
                {
                    LudumCartridge.Ui.ErrorToast.ShowError("No action to do there");
                }
                else if (PlayerStats.Energy.CanAfford(content.UpgradeCost()))
                {
                    if (!farmerIsStandingOnTappedTile)
                    {
                        LudumCartridge.Ui.Inventory.ClearGrabbedCard();
                        farmer.EnqueueGoToTile(position);
                    }

                    farmer.EnqueueUpgradeCurrentTile();
                }
                else
                {
                    LudumCartridge.Ui.ErrorToast.ShowError("Not enough Energy");
                }
            }
        };

        guy.Transform.Position = new Vector2(500, 500);
    }

    public Tiles Tiles { get; }

    public Garden Garden { get; }

    public Scene Scene { get; }
}
