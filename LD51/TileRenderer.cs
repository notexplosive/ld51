﻿using ExplogineMonoGame;
using ExplogineMonoGame.AssetManagement;
using ExplogineMonoGame.Data;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class TileRenderer : BaseComponent
{
    private readonly Tiles _tiles;

    public TileRenderer(Actor actor) : base(actor)
    {
        _tiles = RequireComponent<Tiles>();
    }

    public override void Draw(Painter painter)
    {
        foreach(var position in _tiles.AllTilesPositions())
        {
            {
                A.TileSheet.DrawFrameAtPosition(painter, _tiles.GetContentAt(position).Frame,
                    position.Rectangle.Location.ToVector2(),
                    Scale2D.One,
                    new DrawSettings {Color = Color.White, Depth = Transform.Depth});
            }
        }
    }
}
