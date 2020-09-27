using Assets.Creature;
using Assets.ServiceLocator;
using Needs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class DrinkWaterFromSource : CreatureTask
{
    public bool Drinking;

    [JsonIgnore]
    private List<Cell> _shoreOptions;

    [JsonIgnore]
    private Cell _targetShore;

    public DrinkWaterFromSource()
    {
    }

    public override string Message
    {
        get
        {
            return $"Drink water from source";
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
                    var thirst = creature.GetNeed<Thirst>();

                    if (thirst.Current >= 80f)
                    {
                        return true;
                    }

                    if (!Drinking)
                    {
                        AddSubTask(new Wait(3, "Drinking...", AnimationType.Interact));
                        Drinking = true;
                    }
                    else
                    {
                        thirst.Current += 25f;
                        Drinking = false;
                    }
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