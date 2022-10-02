using System;
using Microsoft.Xna.Framework;

namespace LD51;

public record TapAction
    (string Title, string Description, TilePosition Position, Color Color, Action Behavior) : ITapAction
{
    public void Execute()
    {
        Behavior();
    }
}
