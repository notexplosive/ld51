using System.Collections;
using System.Collections.Generic;
using ExplogineMonoGame;
using MachinaLite;
using MachinaLite.Components;
using Microsoft.Xna.Framework;

namespace LD51;

public class Deck : BaseComponent
{
    private Stack<CropTemplate> _content = new();
    private readonly Box _box;

    public Deck(Actor actor) : base(actor)
    {
        _box = RequireComponent<Box>();
    }

    public int NumberOfCards => _content.Count;
    public Rectangle Rectangle => _box.Rectangle;

    public void AddCard(CropTemplate template)
    {
        _content.Push(template);
    }

    public bool IsNotEmpty()
    {
        return _content.Count > 0;
    }
    
    public bool IsEmpty()
    {
        return _content.Count == 0;
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
