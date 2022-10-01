using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class World
{
    public World(Scene scene)
    {
        var gardenActor = scene.AddActor("Tiles");
        var tiles = new Tiles(gardenActor, new Point(25));
        new TileRenderer(gardenActor);
        new SelectedTileRenderer(gardenActor);
        var garden = new Garden(gardenActor, tiles);
        new GardenRenderer(gardenActor);
        gardenActor.Transform.Depth += 500;

        var guy = scene.AddActor("Guy");
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
            else if (LudumCartridge.Ui.Inventory.HasGrabbedCard())
            {
                var canPlantHere = tiles.GetContentAt(position).IsWet && garden.IsEmpty(position);
                if (canPlantHere)
                {
                    var crop = LudumCartridge.Ui.Inventory.GrabbedCard.CropTemplate;
                    LudumCartridge.Ui.Inventory.Discard(LudumCartridge.Ui.Inventory.GrabbedCard, LudumCartridge.Ui.DiscardPile);
                    LudumCartridge.Ui.Inventory.ClearGrabbedCard();

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
                    LudumCartridge.Ui.Inventory.ClearGrabbedCard();
                }
            }
            else
            {
                farmer.ClearTween();

                if (!farmerIsStandingOnTappedTile)
                {
                    LudumCartridge.Ui.Inventory.ClearGrabbedCard();
                    farmer.EnqueueGoToTile(position);
                }

                farmer.EnqueueUpgradeCurrentTile();
            }
        };

        guy.Transform.Position = new Vector2(500, 500);
    }
}
