
namespace Assets.Creature.Behaviour
{
    public interface IBehaviour
    {
        CreatureTask GetTask(CreatureData creature);
    }
}