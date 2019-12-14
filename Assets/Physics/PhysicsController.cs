using System.Collections.Generic;
using UnityEngine;

public class PhysicsController : MonoBehaviour
{
    internal Queue<Cell> VolatileCells = new Queue<Cell>();

    public int UpdatesPerFrame = 200;

    [Range(0f, 0.2f)] public float PhysicsRate = 0.05f;
    internal float WorkTick;

    //private EntityManager _entityManager;

    // public Entity ManaTiles;

    public void Start()
    {
        //_entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //var entities = new NativeArray<Entity>(Game.Map.Width * Game.Map.Height, Allocator.Temp);
        //var archetype = _entityManager.CreateArchetype(typeof(ManaTile));

        //_entityManager.CreateEntity(archetype, entities);

        //var i = 0;
        //for (int x = 0; x < Game.Map.Width; x++)
        //{
        //    for (int y = 0; y < Game.Map.Height; y++)
        //    {
        //        var e = entities[i];
        //        _entityManager.SetComponentData(e, new ManaTile
        //        {
        //            Amount = 0,
        //            Color = ManaColor.Black,
        //            Coords = new int2(x, y)
        //        });
        //        i++;
        //    }
        //}

        //ManaTiles = entities[0];
    }

    // Update is called once per frame
    private void Update()
    {
        if (!Game.Ready)
            return;
        if (Game.TimeManager.Paused)
            return;

        WorkTick += Time.deltaTime;
        if (WorkTick < PhysicsRate)
        {
            return;
        }

        WorkTick = 0;

        if (VolatileCells.Count == 0)
        {
            return;
        }

        var batch = new List<Cell>();
        for (int i = 0; i < UpdatesPerFrame; i++)
        {
            if (VolatileCells.Count == 0 || VolatileCells.Peek() == null)
            {
                break;
            }
            batch.Add(VolatileCells.Dequeue());
        }

        foreach (var cell in batch)
        {
            cell.UpdatePhysics();
        }
    }

    internal void Track(Cell cell)
    {
        if (!VolatileCells.Contains(cell))
        {
            VolatileCells.Enqueue(cell);
        }
    }
}