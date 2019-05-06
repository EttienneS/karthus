using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public interface ICreatureSprite
{
    void Update();

    Sprite GetIcon();
}
