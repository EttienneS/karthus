using Assets.Map;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PathRequest
{
    private bool _invalid;
    private List<IPathFindableCell> _path;

    public PathRequest(IPathFindableCell from, IPathFindableCell to, Mobility mobility)
    {
        From = from;
        To = to;
        Mobility = mobility;
    }

    public IPathFindableCell From { get; set; }
    public Mobility Mobility { get; set; }
    public IPathFindableCell To { get; set; }

    public List<IPathFindableCell> GetPath()
    {
        return _path;
    }

    public void MarkPathInvalid()
    {
        Debug.LogWarning($"No path found from {From} to {To} for an entity that {Mobility}s");
        _invalid = true;
    }

    public void PopulatePath(List<IPathFindableCell> path)
    {
        _path = path;
    }

    public bool Ready()
    {
        if (_invalid)
        {
            throw new InvalidPathException(this);
        }
        return _path != null;
    }
}