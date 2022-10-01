using System.Collections.Generic;
using MachinaLite;

namespace LD51;

public class Garden : BaseComponent
{
    private readonly Dictionary<TilePosition, Crop> _map = new();
    private readonly Tiles _tiles;

    public Garden(Actor actor, Tiles tiles) : base(actor)
    {
        _tiles = tiles;
    }

    public void PlantCrop(CropTemplate template, TilePosition position)
    {
        var crop = template.CreateCrop(this,position, _tiles);
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

    public bool HasCropAt(TilePosition position)
    {
        return _map.ContainsKey(position);
    }

    public void RemoveCropAt(TilePosition position)
    {
        _map.Remove(position);
    }
}
