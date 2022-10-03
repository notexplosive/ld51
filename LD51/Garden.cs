using System.Collections.Generic;
using MachinaLite;
using Microsoft.Xna.Framework;

namespace LD51;

public class Garden : BaseComponent
{
    private readonly Dictionary<Point, Crop> _map = new();

    public Garden(Actor actor) : base(actor)
    {
    }

    public void PlantCrop(CropEventData data)
    {
        var crop = data.Template.CreateCrop(data);
        _map[data.Position.GridPosition] = crop;
    }

    public bool IsEmpty(TilePosition tilePosition)
    {
        return !_map.ContainsKey(tilePosition.GridPosition);
    }

    public IEnumerable<Point> FilledPositions()
    {
        return _map.Keys;
    }

    public Crop GetCropAt(TilePosition position)
    {
        return _map[position.GridPosition];
    }
    
    public Crop GetCropAt(Point gridPosition)
    {
        return _map[gridPosition];
    }

    public IEnumerable<Crop> AllCrops()
    {
        return _map.Values;
    }

    public bool HasAnyCrop()
    {
        return _map.Count > 0;
    }

    public bool HasCropAt(TilePosition position)
    {
        return _map.ContainsKey(position.GridPosition);
    }
    
    public bool HasCropAt(Point gridPosition)
    {
        return _map.ContainsKey(gridPosition);
    }

    public void RemoveCropAt(TilePosition position)
    {
        _map.Remove(position.GridPosition);
    }

    public void KillAllCrops()
    {
        _map.Clear();
    }

    public IEnumerable<Crop> AllWateredCrops()
    {
        foreach (var kv in _map)
        {
            var position = kv.Key;
            if (LudumCartridge.World.Tiles.GetContentAt(position).IsWet)
            {
                yield return kv.Value;
            }
        }
    }

    public IEnumerable<Crop> AllReadyCrops()
    {
        foreach (var crop in _map.Values)
        {
            if (crop.IsReadyToHarvest)
            {
                yield return crop;
            }
        }
    }
}
