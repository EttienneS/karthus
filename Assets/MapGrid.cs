using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour
{
    public Cell[] cells;

    public int Width = 15;
    public int Height = 15;

    // Start is called before the first frame update
    void Start()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var tile = Instantiate(cells[Random.Range(0, cells.Length)], transform, true);
                tile.transform.position = new Vector3(x, y);
                tile.name = $"Map {x}:{y}";
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
