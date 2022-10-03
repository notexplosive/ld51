using System.Collections.Generic;
using ExplogineMonoGame;

namespace LD51;

public class RarityBag
{
    private static readonly Dictionary<Rarity, RarityBag> Instances = new();
    private readonly Rarity _rarity;
    private List<CropTemplate> _cache;

    private RarityBag(Rarity rarity)
    {
        _rarity = rarity;
        _cache = GetAllCropsOfRarity(_rarity);
    }

    public List<CropTemplate> GetAllCropsOfRarity(Rarity rarity)
    {
        var templates = new List<CropTemplate>();
        foreach (var template in CropTemplate.AllTemplates)
        {
            if (template.Rarity == rarity)
            {
                templates.Add(template);
            }
        }

        Client.Random.Clean.Shuffle(templates);

        return templates;
    }

    public static RarityBag Get(Rarity rarity)
    {
        if (!RarityBag.Instances.ContainsKey(rarity))
        {
            RarityBag.Instances[rarity] = new RarityBag(rarity);
        }

        return RarityBag.Instances[rarity];
    }

    public CropTemplate Pull()
    {
        if (_cache.Count == 0)
        {
            _cache = GetAllCropsOfRarity(_rarity);
        }

        var result = _cache[0];
        _cache.RemoveAt(0);

        return result;
    }
}
