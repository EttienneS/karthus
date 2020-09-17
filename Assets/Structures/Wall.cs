using Assets.Structures.Behaviour;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Assets.Structures
{
    

    public class Wall : Structure
    {
        public (int x, int z) CellWallCoords;

        public Cell GetCellWall()
        {
            return Map.Instance.GetCellAtCoordinate(CellWallCoords.x, CellWallCoords.z);
        }

        public override void Load()
        {
            base.Load();

            var n = Cell.NonNullNeighbors.GetRandomItem();
            CellWallCoords = (n.X, n.Z);
            if (Cell != null)
            {
                var direction = Cell.GetDirectionOfNeighbor(GetCellWall());
                Rotation = (int)direction * 90f;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

        }
    }
}