using System.Collections;
using System.Collections.Generic;
using ExplogineMonoGame;
using MachinaLite;

namespace LD51;

public class Deck : BaseComponent
{
    private Stack<CropTemplate> _content = new();
    
    public Deck(Actor actor) : base(actor)
    {
        AddCard(CropTemplate.Potato);
        AddCard(CropTemplate.Potato);
        AddCard(CropTemplate.Potato);
        AddCard(CropTemplate.Potato);
        AddCard(CropTemplate.Potato);
    }

    public int NumberOfCards => _content.Count;

    public void AddCard(CropTemplate template)
    {
        _content.Push(template);
    }

    public bool IsNotEmpty()
    {
        return _content.Count > 0;
    }

    public CropTemplate NextTemplate()
    {
        return _content.Pop();
    }

    public void Shuffle()
    {
        var buffer = new List<CropTemplate>();
        while (_content.Count > 0)
        {
            buffer.Add(_content.Pop());
        }
        
        Client.Random.Clean.Shuffle(buffer);

        foreach (var item in buffer)
        {
            _content.Push(item);
        }
    }
}
