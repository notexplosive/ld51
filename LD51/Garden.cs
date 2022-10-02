using System.Collections.Generic;
using MachinaLite;

namespace LD51;

public class Garden : BaseComponent
{
    private readonly Dictionary<TilePosition, Crop> _map = new();

    public Garden(Actor actor) : base(actor)
    {
    }

    public void PlantCrop(CropEventData data)
    {
        var crop = data.Template.CreateCrop(data);
        _map[data.Position] = crop;
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

    public bool HasAnyCrop()
    {
        return _map.Count > 0;
    }

    public bool HasCropAt(TilePosition position)
    {
        return _map.ContainsKey(position);
    }

    public void RemoveCropAt(TilePosition position)
    {
        _map.Remove(position);
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
