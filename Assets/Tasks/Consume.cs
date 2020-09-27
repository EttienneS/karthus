using Assets.Creature;
using Assets.Item;
using Assets.ServiceLocator;
using Needs;
using System.Linq;

public class Consume : CreatureTask
{
    public bool Consuming;
    public bool Consumed;

    public string ConsumptionCriteria;

    public override string Message
    {
        get
        {
            return Consuming ? $"Consuming {ConsumptionCriteria}" : $"Getting {ConsumptionCriteria} to consume";
        }
    }

    public Consume()
    {

    }

    public Consume(string consumptionCriteria) : this()
    {
        ConsumptionCriteria = consumptionCriteria;
    }

    public override void FinalizeTask()
    {
    }

    public Consume(ItemData food) : this(food.Name)
    {
        AddSubTask(new Pickup(food, 1));
    }

    public bool FoundSeating;
    public string ChairId;

    public override bool Done(CreatureData creature)
    {
        if (SubTasksComplete(creature))
        {
            var food = creature.HeldItem;

            if (food == null || !food.IsType(ConsumptionCriteria))
            {
                creature.DropItem(creature.Cell);
                AddSubTask(new FindAndGetItem(ConsumptionCriteria, 1));
                return false;
            }

            if (!FoundSeating)
            {
                FoundSeating = true;
                var chair = creature.Faction.Structures
                                 .Where(s => s.IsType("Chair") && !s.InUseByAnyone)
                                 .OrderBy(c => c.Cell.DistanceTo(creature.Cell))
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

            if (!Consuming)
            {
                AddSubTask(new Wait(2, "Eating...", AnimationType.Interact));
                BusyEmote = "*munch, chomp*";
                Consuming = true;
            }
            else if (!Consumed)
            {
                BusyEmote = "";

                if (food.ValueProperties.ContainsKey("Nutrition"))
                {
                    creature.GetNeed<Hunger>().Current += food.ValueProperties["Nutrition"];
                }
                if (food.ValueProperties.ContainsKey("Quench"))
                {
                    creature.GetNeed<Thirst>().Current += food.ValueProperties["Quench"];
                }

                creature.DropItem(creature.Cell);

                if (!string.IsNullOrEmpty(ChairId))
                {
                    ChairId.GetStructure().Free();
                }
                Loc.GetItemController().DestroyItem(food);
                Consumed = true;

                if (creature.GetNeed<Hunger>().Current < 60)
                {
                    AddSubTask(new Consume());
                }
                else
                {
                    return true;
                }
            }
            else
            {
                Consuming = false;
                Consumed = false;
                return true;
            }
        }
        return false;
    }
}