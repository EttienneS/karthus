using Assets.Creature;
using Assets.Item;
using UnityEditorInternal.Profiling.Memory.Experimental;

namespace Assets.Tasks
{
    public class StoreItem : CreatureTask
    {
        private readonly ItemData _item;
        private (int x, int z) _targetCellCoordinate;

        public StoreItem()
        {
        }

        public StoreItem(ItemData item) : this()
        {
            _item = item;
        }

        public StoreItem(ItemData item, Cell cell) : this(item)
        {
            _targetCellCoordinate = (cell.X, cell.Z);
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
                if (creature.HeldItem != _item)
                {
                    if (_item.Cell == creature.Cell)
                    {
                        creature.PickUpItem(_item, _item.Amount);
                    }
                    else
                    {
                        AddSubTask(new Move(_item.Cell));
                    }
                }
                else if (_targetCellCoordinate == (0, 0))
                {
                    FindStoreCellForItem(creature);
                    AddSubTask(new Move(GetTargetCell()));
                }
                else
                {
                    var targetCell = GetTargetCell();
                    if (creature.Cell == targetCell)
                    {
                        creature.DropItem(targetCell);
                    }
                    else
                    {
                        throw new UnableToFindPathException(targetCell);
                    }
                }
            }

            return true;
        }

        public override void FinalizeTask()
        {
        }

        internal Cell GetTargetCell()
        {
            return Game.Instance.Map.GetCellAtCoordinate(_targetCellCoordinate);
        }

        private Cell FindStoreCellForItem(CreatureData creature)
        {
            foreach (var store in creature.Faction.StorageZones)
            {
                if (store.CanStore(_item))
                {
                    var cell = store.GetReservedCellFor(_item);
                    _targetCellCoordinate = (cell.X, cell.Z);
                }
            }

            throw new NoCellFoundException($"No cell found to store {_item}");
        }
    }
}