using System.Collections.Generic;
using ExplogineMonoGame;
using ExplogineMonoGame.Data;
using ExTween;
using ExTweenMonoGame;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class Cutscene
{
    private readonly CutsceneDeck _deck;
    private readonly TweenableVector2 _deckPosition = new(Vector2.Zero);
    private readonly TweenableFloat _faderOpacity = new(0f);
    private string _text;
    private readonly Tweenable<float> _textOpacity = new TweenableFloat(0f);

    public Cutscene(Scene scene)
    {
        Scene = scene;

        var fader = Scene.AddActor("Fader");
        new Fader(fader, _faderOpacity);
        fader.Transform.Depth += 1000;

        var deckActor = Scene.AddActor("Deck");
        _deck = new CutsceneDeck(deckActor, _deckPosition);

        var startPosition = new Vector2(Client.Window.RenderResolution.X / 2f, Client.Window.RenderResolution.Y + 200);
        _deckPosition.Value = startPosition;

        var textActor = scene.AddActor("Text");
        new Box(textActor, Client.Window.RenderResolution);
        var textInBox = new TextInBox(textActor, A.BigFont, "");
        new Updater(textActor, _ =>
        {
            textInBox.Text = _text;
            textInBox.Color = Color.White.WithMultipliedOpacity(_textOpacity.Value);
        });
    }

    public Scene Scene { get; }

    public SequenceTween Tween { get; } = new();

    public bool IsPlaying()
    {
        return !Tween.IsDone();
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

        for (var i = 0; i < 1; i++)
        {
            Tween.Add(new CallbackTween(() =>
                Fx.PutCardInDiscard(Fx.UiSpaceToGameSpace(ui.ReshuffleButtonBox.Rectangle.Center.ToVector2()),
                    CropTemplate.Potato)));
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

        Tween.Add(new Tween<float>(_faderOpacity, 1f, 0.5f, Ease.Linear));
        Tween.Add(new CallbackTween(() => world.Tiles.RandomizeContent()));

        var startPosition = new Vector2(Client.Window.RenderResolution.X / 2f, Client.Window.RenderResolution.Y + 200);
        _deckPosition.Value = startPosition;
        var centerOfScreen = Client.Window.RenderResolution.ToVector2() / 2;

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
                    Tween.Add(new Tween<float>(_faderOpacity, 0f, 0.5f, Ease.Linear));

                    Tween.Add(new WaitSecondsTween(0.15f));

                    for (var i = 0; i < 4; i++)
                    {
                        Tween.Add(new CallbackTween(() => ui.Inventory.DrawNextCard(true)));
                        Tween.Add(new WaitSecondsTween(0.15f));
                    }
                })
        );
    }

    public void PlayOpening()
    {
        _faderOpacity.Value = 1f;

        void EnqueShowMessage(string message, float lingerDuration = 2f)
        {
            Tween.Add(new CallbackTween(() => _text = message));
            Tween.Add(new Tween<float>(_textOpacity, 1f, 0.5f, Ease.Linear));
            Tween.Add(new WaitSecondsTween(lingerDuration));
            Tween.Add(new Tween<float>(_textOpacity, 0f, 0.5f, Ease.Linear));
            Tween.Add(new CallbackTween(() => _text = ""));
        }

        EnqueShowMessage("Somewhere in the wasteland, 100 years after the Calamity...");
        EnqueShowMessage("A Golem awakens, it's mission carved into its mind.");
        
        Tween.Add(new Tween<float>(_faderOpacity, 0f, 1f, Ease.Linear));
        
        EnqueShowMessage("\"Restore this forgotten wasteland\"", 2);
    }
}
