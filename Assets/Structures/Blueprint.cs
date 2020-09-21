using Assets.Map;
using Newtonsoft.Json;
using Structures;
using System.Linq;

namespace Assets.Structures
{
    public class Blueprint
    {
        public Cost Cost;

        [JsonIgnore]
        public Cell Cell
        {
            get
            {
                return MapController.Instance.GetCellAtCoordinate(Coords.x, Coords.z);
            }
            set
            {
                Coords = (value.X, value.Z);
            }
        }

        public (int x, int z) Coords;
        
        public string StructureName;

        public string FactionName;

        public string ID;

        [JsonIgnore]
        public Build AssociatedBuildTask
        {
            get
            {
                return Faction.AllTasks.OfType<Build>().FirstOrDefault(b => b.Blueprint == this);
            }
        }

        [JsonIgnore]
        public Faction Faction
        {
            get
            {
                return Game.Instance.FactionController.Factions[FactionName];
            }
        }

        [JsonIgnore]
        public BlueprintRenderer BlueprintRenderer;

        public Blueprint()
        {
        }

        public Blueprint(string structureName, Cell cell, Faction faction)
        {
            StructureName = structureName;
            Cost = Game.Instance.StructureController.GetStructureCost(structureName);
            Cell = cell;
            FactionName = faction.FactionName;
            ID = Game.Instance.IdService.GetId();
        }
    }
}