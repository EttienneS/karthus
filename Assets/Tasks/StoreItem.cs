using Assets.Creature;
using Assets.Item;
using UnityEditorInternal.Profiling.Memory.Experimental;

namespace Assets.Tasks
{
    public class StoreItem : CreatureTask
    {
        private readonly string _itemId;
        private (int x, int z) _targetCellCoordinate;

        public StoreItem()
        {
        }

        public StoreItem(ItemData item) : this()
        {
            _itemId = item.Id;
        }

        public override string Message
        {
            get
            {
                return "Moving item to store";
            }
        }

        public override bool Done(CreatureData creature)
        {
            if (SubTasksComplete(creature))
            {
                var item = _itemId.GetItem();
                if (creature.HeldItem != item)
                {
                    if (item.Cell == creature.Cell)
                    {
                        creature.PickUpItem(item, item.Amount);
                    }
                    else
                    {
                        AddSubTask(new Move(item.Cell));
                    }
                }
                else if (_targetCellCoordinate == (0, 0))
                {
                    _targetCellCoordinate = FindStoreCellForItem(creature, item);
                    AddSubTask(new Move(Game.Instance.Map.GetCellAtCoordinate(_targetCellCoordinate)));
                }
                else
                {
                    var targetCell = Game.Instance.Map.GetCellAtCoordinate(_targetCellCoordinate);
                    if (creature.Cell == targetCell)
                    {
                        creature.DropItem(targetCell);
                        return true;
                    }
                    else
                    {
                        throw new UnableToFindPathException(targetCell);
                    }
                }
            }

            return false;
        }

        public override void FinalizeTask()
        {
        }

        private (int x, int z) FindStoreCellForItem(CreatureData creature, ItemData item)
        {
            foreach (var store in creature.Faction.StorageZones)
            {
                if (store.CanStore(item))
                {
                    var cell = store.GetReservedCellFor(item);
                    return (cell.X, cell.Z);
                }
            }

            throw new NoCellFoundException($"No cell found to store {item}");
        }
    }
}