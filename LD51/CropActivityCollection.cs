using System.Collections.Generic;
using System.Text;

namespace LD51;

public class CropActivityCollection
{
    private List<CropActivity> _activities = new();
    private readonly string _verb;

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
        
        var stringBuilder = new StringBuilder();

        stringBuilder.Append($"On {_verb}: ");

        for (var index = 0; index < _activities.Count; index++)
        {
            var item = _activities[index];
            if (index > 0 && index != _activities.Count -1)
            {
                stringBuilder.Append(", ");
            }

            stringBuilder.Append(item.Description);
        }

        return stringBuilder.ToString();
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
