using Assets.Creature;
using Assets.ServiceLocator;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class GetWaterFromSource : CreatureTask
{
    [JsonIgnore]
    private List<Cell> _shoreOptions;

    [JsonIgnore]
    private Cell _targetShore;

    public GetWaterFromSource()
    {
    }

    public override string Message
    {
        get
        {
            return "Get water from source";
        }
    }

    public override bool Done(CreatureData creature)
    {
        try
        {
            if (SubTasksComplete(creature))
            {
                if (MoveToShore(creature))
                {
                    var water = Loc.GetItemController().SpawnItem("FreshWater", creature.Cell, 1);
                    creature.PickUpItem(water, 1);
                    return true;
                }
            }
        }
        catch (TaskFailedException)
        {
            if (_targetShore != null)
            {
                _targetShore = null;
            }
        }

        return false;
    }

    public override void FinalizeTask()
    {
    }

    private void FindShoreOptions(CreatureData creature)
    {
        _shoreOptions = Loc.GetMap().Cells
                           .Where(c => c.BiomeRegion.Name == "Water")
                           .SelectMany(c => c.NonNullNeighbors)
                           .Where(n => n.PathableWith(creature.Mobility))
                           .OrderBy(c => c.DistanceTo(creature.Cell))
                           .ToList();
    }

    private bool MoveToShore(CreatureData creature)
    {
        if (_targetShore == null)
        {
            if (_shoreOptions == null)
            {
                FindShoreOptions(creature);
            }

            _targetShore = _shoreOptions[0];
            _shoreOptions.Remove(_targetShore);
            AddSubTask(new Move(_targetShore));
            return false;
        }
        else
        {
            return true;
        }
    }
}