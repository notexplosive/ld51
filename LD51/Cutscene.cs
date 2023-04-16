using System;
using System.Collections.Generic;
using ExplogineCore.Data;
using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using ExplogineMonoGame.Input;
using ExTween;
using ExTweenMonoGame;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class Cutscene
{
    private readonly IRuntime _runtime;
    private readonly CutsceneDeck _deck;
    private readonly TweenableVector2 _deckPosition = new(Vector2.Zero);
    private readonly TweenableFloat _faderOpacity = new(0f);
    private readonly TweenableFloat _musicVolume = new(0f);
    private readonly Actor _shop;
    private readonly Tweenable<float> _textOpacity = new TweenableFloat(0f);
    private readonly TextInBox _tooltipText;
    private bool _doneShopping;
    private string _text;

    public Cutscene(Scene scene, IRuntime runtime)
    {
        _runtime = runtime;
        Scene = scene;

        var music = Client.SoundPlayer.Play("bgm", new SoundEffectSettings {Loop = true, Volume = 0});
        var music2 = Client.SoundPlayer.Play("bgm2", new SoundEffectSettings {Loop = true, Volume = 0});
        new Updater(Scene.AddActor("Music"), dt =>
        {
            var factor = 4f;
            music.Volume = _musicVolume.Value / factor;
            music2.Volume = (1 - _musicVolume.Value) / factor;
        });

        var fader = Scene.AddActor("Fader");
        new Fader(fader, _faderOpacity, _runtime);
        fader.Transform.Depth += 1000;

        var deckActor = Scene.AddActor("Deck");
        _deck = new CutsceneDeck(deckActor, _deckPosition, _runtime);

        var startPosition = new Vector2(_runtime.Window.RenderResolution.X / 2f, _runtime.Window.RenderResolution.Y + 200);
        _deckPosition.Value = startPosition;

        var textActor = scene.AddActor("Text");
        new Box(textActor, _runtime.Window.RenderResolution);
        var textInBox = new TextInBox(textActor, A.BigFont, "");
        new Updater(textActor, _ =>
        {
            textInBox.Text = _text;
            textInBox.Color = Color.White.WithMultipliedOpacity(_textOpacity.Value);
        });

        _shop = Scene.AddActor("Shop");
        _shop.Visible = false;

        ShopItem[] shopItems =
        {
            new("Buy Collard", CropTemplate.GetByName("Collard").Description, 30,
                (item, pos) => Fx.PutCardInDiscard(pos, CropTemplate.GetByName("Collard"))),

            new("Buy Beet", CropTemplate.GetByName("Beet").Description, 50,
                (item, pos) =>
                {
                    Fx.PutCardInDiscard(pos, CropTemplate.GetByName("Beet"));
                    item.Cost += 25;
                }),

            new("Buy KohlRabi", CropTemplate.GetByName("Kohlrabi").Description, 500,
                (item, pos) =>
                {
                    Fx.PutCardInDiscard(pos, CropTemplate.GetByName("Kohlrabi"));
                    item.Name = "SOLD OUT";
                    item.Description = "SOLD OUT";
                    item.Cost = -1;
                    item.OnBuy = (item, pos) => { };
                }),

            new("Restore\nWasteland",
                "Restores the Wasteland slightly, increasing the amount of usable soil", 100,
                (item, pos) =>
                {
                    LudumCartridge.World.Tiles.UsableSoilPercent += 0.1f;
                    item.Cost += 100;

                    if (LudumCartridge.World.Tiles.UsableSoilPercent > 1f)
                    {
                        item.Cost = -1;
                        item.Name = "Fully Restored!";
                        item.Description = "You did it (grow a pumpkin now)";
                        item.OnBuy = (item, pos) => { };
                    }
                })
        };

        var wholeScreen = _runtime.Window.RenderResolution;
        var tooltip = _shop.Transform.AddActorAsChild("Tooltip");
        var tooltipInset = 100;
        tooltip.Transform.Position += new Vector2(tooltipInset);
        new Box(tooltip, wholeScreen - new Point(tooltipInset * 2));
        _tooltipText = new TextInBox(tooltip, A.CardTextFont, "Tooltip text");
        _tooltipText.Alignment = Alignment.TopCenter;
        _tooltipText.Color = Color.White;

        new Updater(tooltip, dt => { _tooltipText.Text = ""; });

        var paddingBetweenItems = 100;
        var allItemsWidth = shopItems.Length * A.CardSize.X + (paddingBetweenItems * shopItems.Length - 1);

        var buttons = _shop.Transform.AddActorAsChild("Buttons");
        buttons.Transform.LocalPosition = wholeScreen.ToVector2() / 2 - new Vector2(allItemsWidth, A.CardSize.Y) / 2;
        var index = 0;
        foreach (var item in shopItems)
        {
            var buyButton = buttons.Transform.AddActorAsChild("Buy Button");
            buyButton.Transform.LocalPosition = new Vector2(index * A.CardSize.X + paddingBetweenItems * index, 0);
            MakeButton(buyButton, A.CardSize, () =>
            {
                var costString = $"{item.Cost} Energy";

                if (item.Cost < 0)
                {
                    costString = "";
                }
                
                return $"{item.Name}\n\n{costString}";
            }, item.Description, () =>
            {
                if (item.Cost < 0)
                {
                    // negative cost means "sold out"
                    return;
                }
                
                if (PlayerStats.Energy.CanAfford(item.Cost))
                {
                    PlayerStats.Energy.Consume(item.Cost);
                    item.OnBuy(item, buyButton.GetComponent<Box>()!.Rectangle.Center.ToVector2());
                    Client.SoundPlayer.Play("stapler", new SoundEffectSettings {Volume = 0.5f, Pitch = 1f});
                }
                else
                {
                    Client.SoundPlayer.Play("blem", new SoundEffectSettings());
                }
            });
            index++;
        }

        {
            var doneShoppingButton = _shop.Transform.AddActorAsChild("DoneShopping");
            var doneButtonSize = new Point(200, 100);
            var doneButtonPosition = wholeScreen.ToVector2() / 2 - doneButtonSize.ToVector2() / 2;
            doneButtonPosition.Y += 300;
            doneShoppingButton.Transform.LocalPosition = doneButtonPosition;
            MakeButton(doneShoppingButton, doneButtonSize, ()=>"Done", null, () => { _doneShopping = true; });
        }
    }

    public Scene Scene { get; }

    public SequenceTween Tween { get; } = new();

    public void MakeButton(Actor buttonActor, Point size, Func<string> labelCallback, string textForTooltip, Action onClick)
    {
        var buyBox = new Box(buttonActor, size);
        var patch = new NinepatchRenderer(buttonActor, Client.Assets.GetAsset<NinepatchSheet>("Card-Patch"));
        var text = new TextInBox(buttonActor, A.CardTextFont, labelCallback());
        new Hoverable(buttonActor);
        var hovered = false;
        new ChangeOnHovered(buttonActor,
            () =>
            {
                if (!hovered)
                {
                    Client.SoundPlayer.Play("draw-card");
                }

                hovered = true;
            },
            () => hovered = false);

        new Updater(buttonActor,
            dt =>
            {
                text.Text = labelCallback();
                if (hovered)
                {
                    patch.Offset = new Point(0, -25);
                    text.Offset = new Point(0, -25);
                    text.Color = Color.White;
                    if (textForTooltip != null)
                    {
                        _tooltipText.Text = textForTooltip;
                    }
                }
                else
                {
                    patch.Offset = new Point(0, 0);
                    text.Offset = new Point(0, 0);
                    text.Color = Color.Black;
                }
            });

        new Clickable(buttonActor).Clicked += button =>
        {
            if (button == MouseButton.Left)
            {
                onClick();
            }
        };
    }

    public bool IsPlaying()
    {
        return !Tween.IsDone();
    }

    public void GoToShop()
    {
        Tween.Add(
            new MultiplexTween()
                .AddChannel(new Tween<float>(_musicVolume, 0f, 0.5f, Ease.Linear))
                .AddChannel(new Tween<float>(_faderOpacity, 0.5f, 0.25f, Ease.Linear))
        );

        Tween.Add(new CallbackTween(() =>
        {
            _shop.Visible = true;
            _doneShopping = false;
        }));
        Tween.Add(new WaitUntilTween(() => _doneShopping));
        Tween.Add(new CallbackTween(() =>
        {
            _shop.Visible = false;
            PlayReshuffle();
        }));
    }

    public void PlayReshuffle()
    {
        var ui = LudumCartridge.Ui;
        var world = LudumCartridge.World;

        Tween.Add(new CallbackTween(() => ui.Inventory.ClearGrabbedCard()));

        Tween.Add(new CallbackTween(() => Fx.GainEnergy(world.GetFarmerPosition(), 25)));

        for (var i = 0; i < ui.Inventory.Count; i++)
        {
            Tween.Add(new CallbackTween(() =>
            {
                var card = ui.Inventory.GetCard(0);
                var template = card.CropTemplate;
                ui.Inventory.Remove(card);
                LudumCartridge.Ui.DiscardPile.Add(template);
            }));
            Tween.Add(new WaitSecondsTween(0.15f));
        }

        Tween.Add(new WaitSecondsTween(0.25f));

        for (var i = 0; i < ui.Deck.NumberOfCards; i++)
        {
            Tween.Add(new CallbackTween(() =>
            {
                var template = ui.Deck.NextTemplate();
                LudumCartridge.Ui.DiscardPile.Add(template);
            }));
            Tween.Add(new WaitSecondsTween(0.1f));
        }

        Tween.Add(new WaitSecondsTween(1f));

        Tween.Add(new CallbackTween(() => world.Garden.KillAllCrops()));
        Tween.Add(new WaitSecondsTween(0.05f));
        Tween.Add(new CallbackTween(() => world.Tiles.DrainAllSoil()));
        Tween.Add(new WaitSecondsTween(0.05f));
        Tween.Add(new CallbackTween(() => world.Tiles.DrainAllSoil()));
        Tween.Add(new WaitSecondsTween(0.05f));
        Tween.Add(new CallbackTween(() => world.Tiles.DrainAllSoil()));
        Tween.Add(new WaitSecondsTween(0.05f));
        Tween.Add(new CallbackTween(() => world.Tiles.DrainAllSoil()));

        Tween.Add(new WaitSecondsTween(1));

        Tween.Add(
            new MultiplexTween()
                .AddChannel(new Tween<float>(_musicVolume, 0f, 0.5f, Ease.Linear))
                .AddChannel(new Tween<float>(_faderOpacity, 1f, 0.5f, Ease.Linear))
        );

        Tween.Add(new CallbackTween(() => world.Tiles.RandomizeContent()));

        var startPosition = new Vector2(_runtime.Window.RenderResolution.X / 2f, _runtime.Window.RenderResolution.Y + 200);
        _deckPosition.Value = startPosition;
        var centerOfScreen = _runtime.Window.RenderResolution.ToVector2() / 2;

        Tween.Add(new Tween<Vector2>(_deckPosition, centerOfScreen, 1f, Ease.CubicFastSlow));

        Tween.Add(
            // this has to be in a callback because numberOfCardsInDiscard changed during the tween
            new CallbackTween(
                () =>
                {
                    var numberOfCardsInDiscard = ui.DiscardPile.Count;
                    var indices = new List<int>();
                    for (var i = 0; i < numberOfCardsInDiscard; i++)
                    {
                        indices.Add(i);
                    }

                    Client.Random.Dirty.Shuffle(indices);

                    Tween.Add(new CallbackTween(() => _deck.NumberOfCards = numberOfCardsInDiscard));

                    var totalDuration = 1f;
                    var durationPerCard = totalDuration / numberOfCardsInDiscard;

                    for (var i = 0; i < numberOfCardsInDiscard; i++)
                    {
                        Tween.Add(_deck.ShootCard(indices[i]));
                        Tween.Add(new WaitSecondsTween(durationPerCard));
                    }

                    Tween.Add(new WaitSecondsTween(0.5f));

                    Client.Random.Dirty.Shuffle(indices);

                    for (var i = 0; i < numberOfCardsInDiscard; i++)
                    {
                        Tween.Add(_deck.PullCardIn(indices[i]));
                        Tween.Add(new WaitSecondsTween(durationPerCard));
                    }

                    Tween.Add(new CallbackTween(() => ui.DiscardPile.Reshuffle()));
                    Tween.Add(new WaitSecondsTween(1));
                    Tween.Add(new Tween<Vector2>(_deckPosition, startPosition, 1f, Ease.CubicSlowFast));
                    Tween.Add(new WaitSecondsTween(1));

                    Tween.Add(
                        new MultiplexTween()
                            .AddChannel(new Tween<float>(_musicVolume, 1f, 0.5f, Ease.Linear))
                            .AddChannel(new Tween<float>(_faderOpacity, 0f, 0.5f, Ease.Linear))
                    );

                    Tween.Add(new WaitSecondsTween(0.15f));

                    for (var i = 0; i < 4; i++)
                    {
                        Tween.Add(new CallbackTween(() => ui.Inventory.DrawNextCard(true)));
                        Tween.Add(new WaitSecondsTween(0.15f));
                    }
                })
        );
    }

    private void EnqueueShowMessage(string message, float lingerDuration = 2f)
    {
        Tween.Add(new CallbackTween(() => _text = message));
        Tween.Add(new Tween<float>(_textOpacity, 1f, 0.5f, Ease.Linear));

        if (lingerDuration > 0)
        {
            Tween.Add(new WaitSecondsTween(lingerDuration));
            Tween.Add(new Tween<float>(_textOpacity, 0f, 0.5f, Ease.Linear));
            Tween.Add(new CallbackTween(() => _text = ""));
        }
    }

    public void PlayOpening(bool skip)
    {
        if (!skip)
        {
            _faderOpacity.Value = 1f;

            EnqueueShowMessage("Somewhere in the wasteland, 100 years after the Calamity...");
            EnqueueShowMessage("A Golem awakens, it's mission carved into its mind.");

            Tween.Add(new Tween<float>(_faderOpacity, 0f, 1f, Ease.Linear));
        }

        Tween.Add(new CallbackTween(() =>
        {
            var fadeInActor = Scene.AddActor("musicFadeIn");
            new TweenOwner(fadeInActor).Tween = new SequenceTween()
                    .Add(new Tween<float>(_musicVolume, 1f, 5f, Ease.Linear))
                    .Add(new CallbackTween(fadeInActor.Destroy))
                ;
        }));

        EnqueueShowMessage("\"Restore this forgotten wasteland\"");
    }

    public void PlayEnding()
    {
        Tween.Add(new Tween<float>(_faderOpacity, 1f, 1f, Ease.Linear));

        EnqueueShowMessage("Somewhere in the wasteland, over 100 years after the Calamity...");
        EnqueueShowMessage("A second Golem awakens.");
        EnqueueShowMessage("Soon there will be an army.");
        EnqueueShowMessage("Finally, after over 100 years...", 3f);
        EnqueueShowMessage("My plan has come to fruition.");
        EnqueueShowMessage("The End - Thanks for playing!\nMade in 72 hours by NotExplosive\nMusic by Crashtroid", -1);
    }
}

public record ShopItem(string Name, string Description, int Cost, Action<ShopItem, Vector2> OnBuy)
{
    public string Name { get; set; } = Name;
    public string Description { get; set; } = Description;
    public int Cost { get; set; } = Cost;
    public Action<ShopItem, Vector2> OnBuy { get; set; } = OnBuy;
}
