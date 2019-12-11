using Newtonsoft.Json;

public class EssenceShatter : CreatureTask
{
    public EssenceShatter()
    {
        RequiredSkill = "Arcana";
        RequiredSkillLevel = 1;
    }

    public string ItemId;

    private Item _item;

    [JsonIgnore]
    public Item Item
    {
        get
        {
            if (_item == null)
            {
                _item = ItemId.GetItem();
            }
            return _item;
        }
    }

    public EssenceShatter(Item item) : this()
    {
        ItemId = item.Id;
        AddSubTask(new Move(item.Cell));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            foreach (var kvp in Item.ManaValue)
            {
                for (int i = 0; i < Item.Amount; i++)
                {
                    creature.ManaPool.GainMana(kvp.Key, kvp.Value);
                    Game.VisualEffectController.SpawnLightEffect(creature,
                                                                 creature.Vector,
                                                                 kvp.Key.GetActualColor(),
                                                                 1.5f,
                                                                 kvp.Value, 3);
                }
            }
            Game.ItemController.DestroyItem(Item);
            return true;
        }

        return false;
    }
}