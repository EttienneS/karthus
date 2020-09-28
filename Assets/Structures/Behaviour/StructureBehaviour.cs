namespace Assets.Structures.Behaviour
{
    public abstract class StructureBehaviour
    {
        private Structure _structure;

        internal Structure GetStructure()
        {
            if (_structure == null)
            {
                _structure = StructureId.GetStructure();
            }
            return _structure;
        }

        public string StructureId { get; set; }

        public void Link(Structure structure)
        {
            StructureId = structure.Id;
        }

        public abstract void Update(float delta);
    }
}