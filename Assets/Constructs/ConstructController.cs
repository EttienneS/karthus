using Newtonsoft.Json;
using System.Collections.Generic;

public static class ConstructController
{
    private static List<Construct> _constructs;

    public static List<Construct> Constructs
    {
        get
        {
            if (_constructs == null)
            {
                _constructs = new List<Construct>();
                foreach (var constructFile in Game.FileController.ConstructFiles)
                {
                    _constructs.Add(LoadConstruct(constructFile.text));
                }
            }

            return _constructs;
        }
    }

    private static Construct LoadConstruct(string data)
    {
        var construct = JsonConvert.DeserializeObject<Construct>(data, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Ignore,
        });

        return construct;
    }
}