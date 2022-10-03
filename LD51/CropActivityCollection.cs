using System.Collections.Generic;

namespace LD51;

public class CropActivityCollection
{
    private readonly string _verb;
    private readonly List<CropActivity> _activities = new();

    public bool Contains(CropActivity target)
    {
        foreach (var activity in _activities)
        {
            if (activity.Description == target.Description)
            {
                return true;
            }
        }

        return false;
    }

    public CropActivityCollection(string verb)
    {
        _verb = verb;
    }

    public string Description()
    {
        if (_activities.Count == 0)
        {
            return "";
        }

        var descriptions = new List<string>();

        foreach (var item in _activities)
        {
            descriptions.Add(item.Description);
        }

        return $"{_verb}: {string.Join(" and ", descriptions)}";
    }

    public void Add(CropActivity activity)
    {
        _activities.Add(activity);
    }

    public void Run(CropEventData self, CropEventData other)
    {
        foreach (var item in _activities)
        {
            item.Behavior(self, other);
        }
    }
}
