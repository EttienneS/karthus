using Assets.Map;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Assets
{
    public class ManagedCellCollection
    {
        [JsonIgnore]
        private List<Cell> _cells;

        private string _cellString;

        public ManagedCellCollection()
        {
            _cells = new List<Cell>();
        }

        public string CellString
        {
            get
            {
                if (_cells == null || _cells.Count == 0)
                {
                    return string.Empty;
                }
                return GetCells().Select(c => c.X + ":" + c.Z).Aggregate((s1, s2) => s1 + "," + s2);
            }
            set
            {
                _cellString = value;
            }
        }

        public void AddCells(IEnumerable<Cell> cells)
        {
            AddCells(cells.ToArray());
        }

        public void AddCells(params Cell[] cells)
        {
            EnsureCellsLoaded();

            _cells.AddRange(cells);
            _cells = _cells.Distinct().ToList();
        }

        public List<Cell> GetCells()
        {
            EnsureCellsLoaded();
            return _cells;
        }

        private void EnsureCellsLoaded()
        {
            if (_cells?.Count == 0 && !string.IsNullOrEmpty(_cellString))
            {
                foreach (var xy in _cellString.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    var split = xy.Split(':').Select(i => int.Parse(i)).ToList();
                    _cells.Add(MapController.Instance.GetCellAtCoordinate(split[0], split[1]));
                }
            }
        }

        internal void RemoveCell(Cell cell)
        {
            EnsureCellsLoaded();

            _cells.Remove(cell);
        }
    }
}