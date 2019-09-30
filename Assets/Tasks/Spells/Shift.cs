using System.Collections.Generic;
using UnityEngine;

public class PipeConstants
{
    public const string Nothing = "Nothing";
    public const string Content = "Content";
    public const string Pressure = "Pressure";
    public const string Suckable = "Suckable";
}

public class Shift : SpellBase
{
    private Pipe _pipe;

    public Shift()
    {
    }

    public override bool DoSpell()
    {
        if (_pipe == null)
        {
            _pipe = Structure as Pipe;
        }

        if (_pipe == null)
        {
            return true;
        }

        _pipe.Flow();

      

        return true;
    }

    
}