using System.Collections.Generic;

namespace Assets.Tasks
{
    public class Blast : TaskBase
    {
        public Blast()
        {
        }

        public IEntity Target;

        public Blast(IEntity target)
        {

            Target = target;
        }

        public override bool Done()
        {
            return false;
        }
    }
}