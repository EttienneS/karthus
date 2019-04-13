using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Construct
{
    public string Floor;
    public Dictionary<char, string> Key;
    public string Name;
    public List<string> Plan;
    public string SpriteName;

    [JsonIgnore]
    public Sprite Sprite
    {
        get
        {
            return Game.SpriteStore.GetSpriteByName(SpriteName);
        }
    }
}