using System.Collections;
using System.Collections.Generic;
using MachinaLite;

namespace LD51;

internal class Garden : BaseComponent
{
    private readonly Dictionary<TilePosition, Crop> _map = new();

    public Garden(Actor actor) : base(actor)
    {
    }

    public void PlantSeed(Crop crop, TilePosition position)
    {
        _map[position] = crop;
    }

    public bool IsEmpty(TilePosition tilePosition)
    {
        return !_map.ContainsKey(tilePosition);
    }

    public IEnumerable<TilePosition> FilledPositions()
    {
        return _map.Keys;
    }

    public Crop GetCropAt(TilePosition position)
    {
        return _map[position];
    }

    public IEnumerable<Crop> AllCrops()
    {
        return _map.Values;
    }
}
