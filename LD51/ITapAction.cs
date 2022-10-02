using Microsoft.Xna.Framework;

namespace LD51;

public interface ITapAction
{
    public Color Color { get; }
    public string Title { get; }
    public string Description { get; }
    void Execute();
}
