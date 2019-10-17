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
                    _constructs.Add(constructFile.text.LoadJson<Construct>());
                }
            }

            return _constructs;
        }
    }
}