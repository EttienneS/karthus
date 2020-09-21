using Assets.Map;
using System.Linq;
using Random = UnityEngine.Random;

namespace Assets.Creature.Behaviour
{
    public class GrazerBehavior : IBehaviour
    {
        public CreatureTask GetTask(CreatureData creature)
        {
            var creatures = creature.Awareness.SelectMany(c => c.Creatures);

            var enemies = creatures.Where(c => c.FactionName != creature.FactionName);
            var herd = creatures.Where(c => c.FactionName == creature.FactionName);

            if (enemies.Any())
            {
                var target = MapController.Instance.GetCellAttRadian(enemies.GetRandomItem().Cell, 10, Random.Range(1, 360));
                return new Move(target);
            }
            else if (herd.Any())
            {
                return new Move(MapController.Instance.GetCircle(herd.GetRandomItem().Cell, 3).GetRandomItem());
            }

            return null;
        }
    }
}