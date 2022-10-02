using System;
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
        Tiles = new Tiles(gardenActor, new Point(25));
        new TileRenderer(gardenActor);
        Garden = new Garden(gardenActor, Tiles);
        new GardenRenderer(gardenActor);
        gardenActor.Transform.Depth += 500;

        var guy = scene.AddActor("Guy");
        var texture = Client.Assets.GetTexture("scarecrow");
        new TextureRenderer(guy, texture, new DrawOrigin(new Vector2(texture.Width / 2f, texture.Height)));
        _farmer = new Farmer(guy, Tiles);

        new SelectedTileRenderer(gardenActor, _farmer);

        Tiles.TileTapped += position =>
        {
            if (_farmer.InputBlocked)
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
    }

    public Tiles Tiles { get; }

    public Garden Garden { get; }

    public Scene Scene { get; }

    public ITapAction GetTapAction(TilePosition position, Card heldCard)
    {
        if (Garden.HasCropAt(position) && Garden.GetCropAt(position).IsReadyToHarvest)
        {
            return new TapAction($"Harvest {Garden.GetCropAt(position).Template.Name}",
                $"{Garden.GetCropAt(position).Template.CropBehaviors.Harvested.Description()}", position, Color.LightGreen,
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
                var newline = description == "" ?  "" : "\n";
                
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
            return new TapError("Fully watered", "Plant something here");
        }

        if (PlayerStats.Energy.CanAfford(content.UpgradeCost()))
        {
            return new TapAction($"{content.UpgradeVerb} {content.Name}", $"Costs {content.UpgradeCost()} Energy",
                position, Color.LightBlue, () =>
                {
                    _farmer.EnqueueGoToTile(position);
                    PlayerStats.Energy.Consume(content.UpgradeCost());
                    _farmer.EnqueueUpgradeCurrentTile();
                });
        }

        return new TapError("Not enough Energy", $"Need {content.UpgradeCost()} Energy to {content.UpgradeVerb}");
    }
}

public interface ITapAction
{
    public Color Color { get; }
    public string Title { get; }
    public string Description { get; }
    void Execute();
}

public record TapAction
    (string Title, string Description, TilePosition Position, Color Color, Action Behavior) : ITapAction
{
    public void Execute()
    {
        Behavior();
    }
}

public record TapError(string Title, string Description) : ITapAction
{
    public void Execute()
    {
        LudumCartridge.Ui.ErrorToast.ShowError(Title);
    }

    public Color Color => Color.DarkRed;
}
