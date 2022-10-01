using System.Collections;
using System.Collections.Generic;
using MachinaLite;

namespace LD51;

public class Deck : BaseComponent
{
    private Stack<CropTemplate> _content = new();
    
    public Deck(Actor actor) : base(actor)
    {
        _content.Push(CropTemplate.Potato);
        _content.Push(CropTemplate.Potato);
        _content.Push(CropTemplate.Potato);
        _content.Push(CropTemplate.Potato);
        _content.Push(CropTemplate.Potato);
    }

    public bool IsNotEmpty()
    {
        return _content.Count > 0;
    }

    public CropTemplate DrawCard()
    {
        return _content.Pop();
    }
}
