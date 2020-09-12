using Newtonsoft.Json;

namespace Assets.Structures.Behaviour
{
    public abstract class StructureBehaviour
    {
        [JsonIgnore]
        internal Structure Structure;

        public StructureBehaviour(Structure structure)
        {
            Structure = structure;
        }

        public abstract void Update(float delta);
    }
}