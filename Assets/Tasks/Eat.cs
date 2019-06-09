public class Eat : TaskBase
{
    public Eat()
    {
    }

    public Eat(string itemCategory)
    {
        //AddSubTask(new GetItem(itemCategory, true, GetItem.SearchBy.Category));
        AddSubTask(new Wait(2f, "Eating"));

        Message = $"Eating {itemCategory}";
    }

    public override bool Done()
    {
        if (Faction.QueueComplete(SubTasks))
        {
           
            return true;
        }

        return false;
    }
}