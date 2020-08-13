using Assets.Creature;
using Assets.Item;
using System.Collections.Generic;
using System.Linq;

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
                var message = $"Moving item '{_itemId}' to store";
                if (_targetCellCoordinate != (0, 0))
                {
                    message += $" at {_targetCellCoordinate.x}:{_targetCellCoordinate.z}";
                }
                return message;
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
                    AddSubTask(new Move(Map.Instance.GetCellAtCoordinate(_targetCellCoordinate)));
                }
                else
                {
                    var targetCell = Map.Instance.GetCellAtCoordinate(_targetCellCoordinate);
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
            var zones = creature.Faction.StorageZones.Where(z => z.CanStore(item));

            if (!zones.Any())
            {
                throw new NoCellFoundException($"No cell found to store {item}");
            }
            else
            {
                var closestZone = FindClosestZone(item.Cell, zones.ToList());
                var cell = closestZone.GetReservedCellFor(item);
                return (cell.X, cell.Z);
            }
        }

        private StorageZone FindClosestZone(Cell itemCell, List<StorageZone> zones)
        {
            var closestZone = zones[0];
            var closestZoneDistance = itemCell.DistanceTo(closestZone.GetLocation());
            foreach (var zone in zones)
            {
                if (zone == closestZone)
                {
                    continue;
                }

                var distance = itemCell.DistanceTo(zone.GetLocation());
                if (distance < closestZoneDistance)
                {
                    closestZone = zone;
                    closestZoneDistance = distance;
                }
            }

            return closestZone;
        }
    }
}