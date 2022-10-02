﻿using System.Collections.Generic;

namespace LD51;

public class CropActivityCollection
{
    private readonly string _verb;
    private readonly List<CropActivity> _activities = new();

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

        return $"On {_verb}: {string.Join(" and ", descriptions)}";
    }

    public void Add(CropActivity activity)
    {
        _activities.Add(activity);
    }

    public void Run(CropEventData data)
    {
        foreach (var item in _activities)
        {
            item.Behavior(data);
        }
    }
}