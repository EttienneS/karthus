using Newtonsoft.Json;
using Structures;
using System.Linq;

namespace Assets.Structures
{
    public class Blueprint
    {
        public Cost Cost;
        public Cell Cell;
        public string StructureName;

        public string FactionName;

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
        }
    }
}