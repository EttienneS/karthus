using Assets.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Map
{
    public class JoinedAreaSearcher
    {
        private readonly int _maxIterations;
        private readonly ChunkCell _origin;
        private readonly CompareMethod _search;
        private HashSet<ChunkCell> _closed;
        private int _iterations;
        private HashSet<ChunkCell> _matches;
        private Queue<ChunkCell> _searchFrontier;

        public JoinedAreaSearcher(ChunkCell origin, CompareMethod compare, int maxIterations = 10000)
        {
            _search = compare;
            _origin = origin;
            _maxIterations = maxIterations;
        }

        public delegate bool CompareMethod(ChunkCell cell);

        public List<ChunkCell> GetPartialResult()
        {
            if (_closed.Count == 0)
            {
                Resolve();
            }
            return _matches.ToList();
        }

        public List<ChunkCell> Resolve()
        {
            using (Instrumenter.Start())
            {
                Reset();
                AddCellToFrontier(_origin);

                while (_searchFrontier.Count > 0)
                {
                    _iterations++;
                    if (_iterations > _maxIterations)
                    {
                        throw new OverflowException($"Search area too large > {_maxIterations} reached!");
                    }

                    var cell = _searchFrontier.Dequeue();
                    if (_search(cell))
                    {
                        _matches.Add(cell);
                        AddNeighboursToSearchFrontier(cell);
                    }
                }
            }

            return _matches.ToList();
        }

        private void AddCellToFrontier(ChunkCell cell)
        {
            _searchFrontier.Enqueue(cell);
            _closed.Add(cell);
        }

        private void AddNeighboursToSearchFrontier(ChunkCell cell)
        {
            foreach (var neighbour in cell.NonNullNeighbors)
            {
                if (_closed.Contains(neighbour))
                    continue;

                AddCellToFrontier(neighbour);
            }
        }

        private void Reset()
        {
            _iterations = 0;
            _searchFrontier = new Queue<ChunkCell>();
            _closed = new HashSet<ChunkCell>();
            _matches = new HashSet<ChunkCell>();
        }
    }
}