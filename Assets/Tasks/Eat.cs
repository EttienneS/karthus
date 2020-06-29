using Needs;
using System.Linq;

public class Eat : CreatureTask
{
    public bool Eating;
    public bool Ate;

    public const string FoodCriteria = "Food";

    public override string Message
    {
        get
        {
            return Eating ? $"Eating {FoodCriteria}" : $"Getting {FoodCriteria} to eat";
        }
    }


    public Eat()
    {
    }

    public override void Complete()
    {
    }

    public Eat(Item food) : this()
    {
        AddSubTask(new Pickup(food, 1));
    }

    public bool FoundSeating;
    public string ChairId;

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var food = creature.HeldItem;

            if (food == null || !food.IsType(FoodCriteria))
            {
                creature.DropItem(creature.Cell);
                AddSubTask(new FindAndGetItem(FoodCriteria, 1));
                return false;
            }

            if (!FoundSeating)
            {
                FoundSeating = true;
                var chair = creature.Faction.Structures
                                 .Where(s => s.IsType("Chair") && !s.InUseByAnyone)
                                 .OrderBy(c => Pathfinder.Distance(c.Cell, creature.Cell, creature.Mobility))
                                 .FirstOrDefault();
                if (chair != null)
                {
                    ChairId = chair.Id;
                    chair.Reserve(creature);
                    AddSubTask(new Move(chair.Cell));
                    return false;
                }
                else
                {
                    creature.Feelings.Add(Feeling.GetAnnoyance("No place to sit and eat"));
                }
            }

            if (!Eating)
            {
                AddSubTask(new Wait(2, "Eating...", AnimationType.Interact));
                BusyEmote = "*munch, chomp*";
                Eating = true;
            }
            else if (!Ate)
            {
                BusyEmote = "";
                creature.GetNeed<Hunger>().Current += food.ValueProperties["Nutrition"];
                creature.DropItem(creature.Cell);

                if (!string.IsNullOrEmpty(ChairId))
                {
                    ChairId.GetStructure().Free();
                }
                Game.Instance.IdService.DestroyEntity(food);
                Ate = true;

                if (creature.GetNeed<Hunger>().Current < 60)
                {
                    AddSubTask(new Eat());
                }
                else
                {
                    return true;
                }
            }
            else
            {
                Eating = false;
                Ate = false;
                return true;
            }
        }
        return false;
    }
}