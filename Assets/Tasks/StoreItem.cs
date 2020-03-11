using Structures;
public class StoreItem : CreatureTask
{
    public string ItemToStoreId { get; set; }
    public string StorageStructureId { get; set; }

    public StoreItem()
    {
        RequiredSkill = SkillConstants.Haul;
        RequiredSkillLevel = 1;
    }

    public StoreItem(Item item, Structure storageStructure) : this()
    {
        ItemToStoreId = item.Id;
        StorageStructureId = storageStructure.Id;

        AddSubTask(new Pickup(item));
        AddSubTask(new Drop(storageStructure.Cell));
    }

    public override bool Done(Creature creature)
    {
        if (SubTasksComplete(creature))
        {
            var item = ItemToStoreId.GetItem();
            if (item == null)
            {
                throw new TaskFailedException($"Item no longer available {ItemToStoreId}");
            }

            var storage = StorageStructureId.GetContainer();
            if (storage == null)
            {
                throw new TaskFailedException($"Structure no longer available {StorageStructureId}");
            }

            storage.AddItem(item);
            return true;
        }
        return false;
    }
}