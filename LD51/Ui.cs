﻿using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExTween;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class Ui
{
    public readonly Tooltip Tooltip;

    public Ui(Scene scene)
    {
        Scene = scene;
        var totalScreenSize = Client.Window.RenderResolution;
        var sheet = Client.Assets.GetAsset<NinepatchSheet>("UI-Patch");

        var resourcesActor = scene.AddActor("Resources");
        var resourcesBox = new Box(resourcesActor, new Point(totalScreenSize.X, 50));
        new NinepatchRenderer(resourcesActor, sheet);
        new Hoverable(resourcesActor);

        var energyText = resourcesActor.Transform.AddActorAsChild("EnergyText");
        new Box(energyText, new Point(200, resourcesBox.Rectangle.Height));
        new ResourceText(energyText, "Energy", PlayerStats.Energy);

        var inventoryActor = scene.AddActor("Inventory");
        inventoryActor.Transform.Position = new Vector2(0, totalScreenSize.Y - A.CardSize.Y * 2f / 3);

        Inventory = new Inventory(inventoryActor);

        var rightPadding = 32;

        // Deck
        var deckActor = inventoryActor.Transform.AddActorAsChild("Deck");
        deckActor.Transform.LocalPosition += new Vector2(totalScreenSize.X - A.CardSize.X - rightPadding, 0);
        deckActor.Transform.LocalDepth -= 10;
        new Box(deckActor, A.CardSize);
        var deck = new Deck(deckActor);
        new Hoverable(deckActor);
        new NinepatchRenderer(deckActor, Client.Assets.GetAsset<NinepatchSheet>("Card-Back"));
        new TextInBox(deckActor, A.CardTextFont, $"Draw Card\n({A.DrawCardCost} Energy)").Color = Color.White;
        new Clickable(deckActor).Clicked += button =>
        {
            if (button == MouseButton.Left)
            {
                if (deck.IsNotEmpty() && PlayerStats.Energy.CanAfford(A.DrawCardCost) && !Inventory.IsFull())
                {
                    PlayerStats.Energy.Consume(A.DrawCardCost);
                    Inventory.AddCard(deck.DrawCard());
                }
            }
        };

        var deckSize = deckActor.Transform.AddActorAsChild("DeckSizeActor");
        new Box(deckSize, new Point(A.CardSize.X, 70));
        var deckSizeText = new TextInBox(deckSize, A.BigFont, "0");
        deckSizeText.Color = Color.White;
        new Updater(deckSize, _ => { deckSizeText.Text = deck.NumberOfCards.ToString(); });

        // Discard Pile
        var discardActor = inventoryActor.Transform.AddActorAsChild("Deck");
        var discardHeaderSize = 32;
        discardActor.Transform.LocalPosition +=
            new Vector2(totalScreenSize.X - A.CardSize.X * 2 - rightPadding * 2, -discardHeaderSize);
        discardActor.Transform.LocalDepth -= 10;
        new Box(discardActor, A.CardSize + new Point(0, discardHeaderSize));
        new Hoverable(discardActor);
        new NinepatchRenderer(discardActor, Client.Assets.GetAsset<NinepatchSheet>("Card-Back"));
        new TextInBox(discardActor, A.BigFont, "0").Color = Color.White;
        var discardHeader = discardActor.Transform.AddActorAsChild("DiscardHeader");
        new Box(discardHeader, new Point(A.CardSize.X, discardHeaderSize));
        new TextInBox(discardHeader, A.UiHintFont, "Discard Pile").Color = Color.White;

        DiscardPile = new DiscardPile(discardActor, deck);

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
                DiscardPile.Reshuffle(deck);
            }
        };

        var inventoryBackground = inventoryActor.Transform.AddActorAsChild("Background");
        inventoryBackground.Transform.LocalPosition += new Vector2(0, 32);
        new Box(inventoryBackground, new Point(totalScreenSize.X, A.CardSize.Y));
        new NinepatchRenderer(inventoryBackground, sheet);
        new Hoverable(inventoryBackground);

        // Tooltip
        var tooltipActor = scene.AddActor("Tooltip");
        tooltipActor.Transform.Depth = 100;
        tooltipActor.Transform.Position = new Vector2(16, 64);
        Tooltip = new Tooltip(tooltipActor);

        // Error toast
        var errorToast = scene.AddActor("Error");
        errorToast.Transform.Depth = 120;
        errorToast.Transform.Position = new Vector2(0, totalScreenSize.Y / 2 - A.BigFont.FontSize / 2);
        ErrorToast = new ErrorToast(errorToast);
    }

    public ErrorToast ErrorToast { get; }

    public Scene Scene { get; }
    public Inventory Inventory { get; }
    public DiscardPile DiscardPile { get; }
}

public class ErrorToast : BaseComponent
{
    private readonly TweenableFloat _opacity = new(0f);
    private string _text;
    private readonly SequenceTween _tween = new();

    public ErrorToast(Actor actor) : base(actor)
    {
    }

    public void ShowError(string errorText)
    {
        _tween.Clear();
        _opacity.Value = 0f;
        _text = errorText;
        _tween.Add(new Tween<float>(_opacity, 1f, 0.25f, Ease.CubicFastSlow));
        _tween.Add(new WaitSecondsTween(1));
        _tween.Add(new Tween<float>(_opacity, 0f, 0.15f, Ease.CubicSlowFast));
    }

    public override void Update(float dt)
    {
        _tween.Update(dt);
    }

    public override void Draw(Painter painter)
    {
        var rect = new Rectangle(Transform.Position.ToPoint(),
            new Point(Client.Window.RenderResolution.X, A.BigFont.FontSize));

        var bgRect = rect;
        bgRect.Inflate(0, 10);
        bgRect.Inflate(0, -10 * _opacity);
        painter.DrawRectangle(
            bgRect,
            new DrawSettings {Color = Color.Black.WithMultipliedOpacity(_opacity.Value / 2f), Depth = Transform.Depth});

        painter.DrawStringWithinRectangle(A.BigFont, _text, rect, Alignment.Center, new DrawSettings
            {
                Color = Color.OrangeRed.WithMultipliedOpacity(_opacity.Value), Depth = Transform.Depth - 1
            }
        );
    }
}
