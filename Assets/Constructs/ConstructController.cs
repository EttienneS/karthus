using System.Collections.Generic;

public class ConstructController
{
    private List<Construct> _constructs;

    public List<Construct> Constructs
    {
        get
        {
            if (_constructs == null)
            {
                _constructs = new List<Construct>();
                foreach (var constructFile in Game.Instance.FileController.ConstructFiles)
                {
                    _constructs.Add(constructFile.text.LoadJson<Construct>());
                }
            }

            return _constructs;
        }
    }
}