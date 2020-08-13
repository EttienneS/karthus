namespace Assets.Creature.Behaviour
{
    public class PersonBehavior : IBehaviour
    {
        public CreatureTask GetTask(CreatureData creature)
        {
            var wound = creature.GetWorstWound();
            if (wound != null)
            {
                return new Heal();
            }
            else if (creature.Cell.Creatures.Count > 1)
            {
                // split up
                return new Move(Map.Instance.TryGetPathableNeighbour(creature.Cell));
            }

            return null;
        }
    }
}