using System;
using System.Collections.Generic;
using ExplogineCore;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Cartridges;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LD51;

public class LudumCartridge : MachinaCartridge
{
    private DiscardPile _discardPile;
    public static Scene GameScene { get; private set; }
    public static Scene UiScene { get; private set; }
    public SeedInventory Inventory { get; private set; }

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
    }

    public override void OnCartridgeStarted()
    {
        A.TileSheet = Client.Assets.GetAsset<SpriteSheet>("Tiles");
        A.TileRect = A.TileSheet.GetSourceRectForFrame(0);

        LudumCartridge.GameScene = AddSceneAsLayer();
        LudumCartridge.UiScene = AddSceneAsLayer();

        Fx.Setup(LudumCartridge.GameScene, LudumCartridge.UiScene);

        BuildGameScene();
        BuildUiScene();

        new DebugComponent(LudumCartridge.GameScene.AddActor("dbug"));
    }

    private void BuildUiScene()
    {
        var totalScreenSize = Client.Window.RenderResolution;
        var sheet = Client.Assets.GetAsset<NinepatchSheet>("UI-Patch");

        var resourcesActor = LudumCartridge.UiScene.AddActor("Resources");
        var resourcesBox = new Box(resourcesActor, new Point(totalScreenSize.X, 50));
        new NinepatchRenderer(resourcesActor, sheet);
        new Hoverable(resourcesActor);

        var energyText = resourcesActor.Transform.AddActorAsChild("EnergyText");
        new Box(energyText, new Point(200, resourcesBox.Rectangle.Height));
        new ResourceText(energyText, "Energy", PlayerStats.Energy);

        var inventoryActor = LudumCartridge.UiScene.AddActor("Inventory");
        inventoryActor.Transform.Position = new Vector2(0, totalScreenSize.Y - A.CardSize.Y * 2f / 3);

        Inventory = new SeedInventory(inventoryActor);
        Inventory.AddCard(CropTemplate.Potato);
        Inventory.AddCard(CropTemplate.Potato);
        Inventory.AddCard(CropTemplate.Potato);

        var rightPadding = 32;

        // Deck
        var deckActor = inventoryActor.Transform.AddActorAsChild("Deck");
        deckActor.Transform.LocalPosition += new Vector2(totalScreenSize.X - A.CardSize.X - rightPadding, 0);
        deckActor.Transform.LocalDepth -= 10;
        new Box(deckActor, A.CardSize);
        var deck = new Deck(deckActor);
        new Hoverable(deckActor);
        new BoxRenderer(deckActor);
        new TextInBox(deckActor, A.CardTextFont, $"Draw Card\n({A.DrawCardCost} Energy)");
        new Clickable(deckActor).Clicked += button =>
        {
            if (button == MouseButton.Left)
            {
                if (deck.IsNotEmpty() && PlayerStats.Energy.CanAfford(A.DrawCardCost))
                {
                    PlayerStats.Energy.Consume(A.DrawCardCost);
                    Inventory.AddCard(deck.DrawCard());
                }
            }
        };

        var deckSize = deckActor.Transform.AddActorAsChild("DeckSizeActor");
        new Box(deckSize, new Point(A.CardSize.X, 70));
        var deckSizeText = new TextInBox(deckSize, A.BigFont, "0");
        new Updater(deckSize, _ => { deckSizeText.Text = deck.NumberOfCards.ToString(); });

        // Discard Pile
        var discardActor = inventoryActor.Transform.AddActorAsChild("Deck");
        var discardHeaderSize = 32;
        discardActor.Transform.LocalPosition +=
            new Vector2(totalScreenSize.X - A.CardSize.X * 2 - rightPadding * 2, -discardHeaderSize);
        discardActor.Transform.LocalDepth -= 10;
        new Box(discardActor, A.CardSize + new Point(0, discardHeaderSize));
        new Hoverable(discardActor);
        new BoxRenderer(discardActor);
        new TextInBox(discardActor, A.BigFont, "0");
        var discardHeader = discardActor.Transform.AddActorAsChild("DiscardHeader");
        new Box(discardHeader, new Point(A.CardSize.X, discardHeaderSize));
        new TextInBox(discardHeader, A.UiHintFont, "Discard Pile");

        _discardPile = new DiscardPile(discardActor, deck);

        // Reshuffle Button
        var reshuffleButtonActor = inventoryActor.Transform.AddActorAsChild("Deck");
        reshuffleButtonActor.Transform.LocalPosition +=
            new Vector2(totalScreenSize.X - A.CardSize.X * 3 - rightPadding * 3, A.CardSize.Y / 4f);
        reshuffleButtonActor.Transform.LocalDepth -= 10;
        new Box(reshuffleButtonActor, new Point(A.CardSize.X, A.CardSize.Y / 4));
        var renderer = new BoxRenderer(reshuffleButtonActor);
        new TextInBox(reshuffleButtonActor, A.CardTextFont, "Reshuffle");
        new Hoverable(reshuffleButtonActor);
        new ChangeOnHovered(
            reshuffleButtonActor,
            () => { renderer.Color = Color.LightBlue; }, 
            () => { renderer.Color = Color.White; }
            );
        new Clickable(reshuffleButtonActor).Clicked += button =>
        {
            if (button == MouseButton.Left)
            {
                _discardPile.Reshuffle(deck);
            }
        };

        var inventoryBackground = inventoryActor.Transform.AddActorAsChild("Background");
        inventoryBackground.Transform.LocalPosition += new Vector2(0, 32);
        new Box(inventoryBackground, new Point(totalScreenSize.X, A.CardSize.Y));
        new NinepatchRenderer(inventoryBackground, sheet);
        new Hoverable(inventoryBackground);
    }

    private void BuildGameScene()
    {
        var gardenActor = LudumCartridge.GameScene.AddActor("Tiles");
        var tiles = new Tiles(gardenActor, new Point(25));
        new TileRenderer(gardenActor);
        new SelectedTileRenderer(gardenActor);
        var garden = new Garden(gardenActor, tiles);
        new GardenRenderer(gardenActor);
        gardenActor.Transform.Depth += 500;

        var guy = LudumCartridge.GameScene.AddActor("Guy");
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
            else if (Inventory.HasGrabbedCard())
            {
                var canPlantHere = tiles.GetContentAt(position).IsWet && garden.IsEmpty(position);
                if (canPlantHere)
                {
                    var crop = Inventory.GrabbedCard.CropTemplate;
                    Inventory.Discard(Inventory.GrabbedCard, _discardPile);
                    Inventory.ClearGrabbedCard();

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
                    Inventory.ClearGrabbedCard();
                }
            }
            else
            {
                farmer.ClearTween();

                if (!farmerIsStandingOnTappedTile)
                {
                    Inventory.ClearGrabbedCard();
                    farmer.EnqueueGoToTile(position);
                }

                farmer.EnqueueUpgradeCurrentTile();
            }
        };

        guy.Transform.Position = new Vector2(500, 500);
    }
}

public class Updater : BaseComponent
{
    private readonly Action<float> _onUpdate;

    public Updater(Actor actor, Action<float> onUpdate) : base(actor)
    {
        _onUpdate = onUpdate;
    }

    public override void Update(float dt)
    {
        _onUpdate(dt);
    }
}

public class DiscardPile : BaseComponent
{
    private readonly TextInBox _text;
    private Stack<CropTemplate> _content = new();
    private readonly Deck _deck;

    public DiscardPile(Actor actor, Deck deck) : base(actor)
    {
        _text = RequireComponent<TextInBox>();
        _deck = deck;
    }

    public override void Update(float dt)
    {
        _text.Text = _content.Count.ToString();
    }

    public void Reshuffle(Deck deck)
    {
        while (_content.Count > 0)
        {
            deck.AddCard(_content.Pop());
        }

        deck.Shuffle();
    }

    public void Add(CropTemplate template)
    {
        _content.Push(template);
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
