using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class World
{
    private readonly Farmer _farmer;

    public World(Scene scene)
    {
        Scene = scene;

        var gardenActor = scene.AddActor("Tiles");
        gardenActor.Transform.Depth += 500;
        Tiles = new Tiles(gardenActor, new Point(30,15));
        new TileRenderer(gardenActor);
        Garden = new Garden(gardenActor);
        new GardenRenderer(gardenActor);

        var guy = scene.AddActor("Guy");
        var texture = Client.Assets.GetTexture("scarecrow");
        new TextureRenderer(guy, texture, new DrawOrigin(new Vector2(texture.Width / 2f, texture.Height)));
        _farmer = new Farmer(guy, Tiles);

        new SelectedTileRenderer(gardenActor, _farmer);

        Tiles.TileTapped += position =>
        {
            if (_farmer.IsAnimating)
            {
                return;
            }

            var heldCard = LudumCartridge.Ui.Inventory.GrabbedCard;
            LudumCartridge.Ui.Inventory.ClearGrabbedCard();
            _farmer.ClearTween();
            GetTapAction(position, heldCard).Execute();
        };

        guy.Transform.Position =
            new Vector2(Client.Window.RenderResolution.X / 2f, Client.Window.RenderResolution.Y / 2f);

        // new CameraPanner(scene.AddActor("CameraPanner"));
    }

    public Tiles Tiles { get; }
    public Garden Garden { get; }
    public Scene Scene { get; }

    public bool FarmerIsBusy()
    {
        return _farmer.IsAnimating;
    }

    public ITapAction GetTapAction(TilePosition position, Card heldCard)
    {
        if (Garden.HasCropAt(position) && Garden.GetCropAt(position).IsReadyToHarvest)
        {
            return new TapAction($"Harvest {Garden.GetCropAt(position).Template.Name}",
                $"{Garden.GetCropAt(position).Template.CropBehaviors.Harvested.Description()}", position,
                Color.LightGreen,
                () =>
                {
                    var crop = Garden.GetCropAt(position);
                    _farmer.EnqueueGoToTile(position);
                    _farmer.EnqueueHarvestCrop(crop);
                });
        }

        var content = Tiles.GetContentAt(position);

        if (heldCard != null)
        {
            if (Tiles.GetContentAt(position).IsWet && Garden.IsEmpty(position))
            {
                var description = heldCard.CropTemplate.CropBehaviors.Planted.Description();
                var newline = description == "" ? "" : "\n";

                return new TapAction($"Plant {heldCard.CropTemplate.Name}",
                    $"Plants crop in {content.Name}{newline}{description}",
                    position, Color.White, () =>
                    {
                        var template = heldCard.CropTemplate;
                        LudumCartridge.Ui.Inventory.Remove(heldCard);
                        _farmer.EnqueueGoToTile(position);
                        _farmer.EnqueuePlantCrop(new CropEventData(position, Garden, template, Tiles));
                    });
            }

            return new TapError("Cannot plant there", "Need Wet Soil");
        }

        if (content.Upgrade() == content)
        {
            if (content.IsWet)
            {
                return new TapError("Fully Watered", "Plant something here");
            }

            return new TapError("Unusable Soil", "Cannot be tilled");
        }

        if (PlayerStats.Energy.CanAfford(content.UpgradeCost()))
        {
            return new TapAction($"{content.UpgradeVerb} {content.Name}", $"Costs {content.UpgradeCost()} Energy",
                position, Color.Cyan, () =>
                {
                    _farmer.EnqueueGoToTile(position);
                    PlayerStats.Energy.Consume(content.UpgradeCost());
                    _farmer.EnqueueUpgradeCurrentTile();
                });
        }

        return new TapError("Not enough Energy", $"Need {content.UpgradeCost()} Energy to {content.UpgradeVerb}");
    }

    public Vector2 GetFarmerPosition()
    {
        return _farmer.Transform.Position;
    }

    public bool IsSoftLocked()
    {
        var ui = LudumCartridge.Ui;
        var world = LudumCartridge.World;

        if (_farmer.IsAnimating)
        {
            return false;
        }

        var noCrops = !world.Garden.HasAnyCrop();
        var cannotPlayAnyCards = ui.Inventory.Count == 0 || !world.Tiles.HasAnyWateredTiles();
        var cannotDraw = !PlayerStats.Energy.CanAfford(A.DrawCardCost) || ui.Deck.IsEmpty();
        var noPendingCrops = world.Garden.AllWateredCrops().ToList().Count == 0 &&
                             world.Garden.AllReadyCrops().ToList().Count == 0;
        var cannotWaterAnyTiles =
            !world.Tiles.HasAnyContent(TileContent.Tilled) || !PlayerStats.Energy.CanAfford(A.WaterCost);
        var cannotTillAnyTiles =
            !world.Tiles.HasAnyContent(TileContent.Dirt) || !PlayerStats.Energy.CanAfford(A.TillCost);

        if (ui.Deck.IsEmpty() && noCrops)
        {
            // More permissive: there's nothing _interesting_ you can do
            return cannotPlayAnyCards && cannotDraw && noPendingCrops;
        }

        // Rock bottom: there are zero actions you can do
        return cannotPlayAnyCards && cannotDraw && noPendingCrops && cannotWaterAnyTiles && cannotTillAnyTiles;
    }
}