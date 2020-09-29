using Assets.ServiceLocator;
using Newtonsoft.Json;
using System.Linq;

namespace Assets.Structures
{
    public class Blueprint
    {
        [JsonIgnore]
        public BlueprintRenderer BlueprintRenderer;

        public (int x, int z) Coords;
        public Cost Cost;

        public string FactionName;

        public string ID;

        public string StructureName;

        public Blueprint()
        {
        }

        public Blueprint(string structureName, Cell cell, Faction faction)
        {
            StructureName = structureName;
            Cost = Loc.GetStructureController().GetStructureCost(structureName);
            Cell = cell;
            FactionName = faction.FactionName;
            ID = Loc.GetIdService().GetId();
        }

        [JsonIgnore]
        public Build AssociatedBuildTask
        {
            get
            {
                return Faction.AllTasks.OfType<Build>().FirstOrDefault(b => b.Blueprint == this);
            }
        }

        [JsonIgnore]
        public Cell Cell
        {
            get
            {
                return Loc.GetMap().GetCellAtCoordinate(Coords.x, Coords.z);
            }
            set
            {
                Coords = (value.X, value.Z);
            }
        }
        [JsonIgnore]
        public Faction Faction
        {
            get
            {
                return Loc.GetFactionController().Factions[FactionName];
            }
        }
    }
}