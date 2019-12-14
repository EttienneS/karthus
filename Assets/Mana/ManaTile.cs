using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public struct ManaTile : IComponentData
{
    public float Amount;
    public ManaColor Color;
    public int2 Coords;
}

public class ManaTileSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new ManaTileJob();
        return job.Schedule(this, inputDeps);
    }

    [BurstCompile]
    private struct ManaTileJob : IJobForEach<ManaTile>
    {
        public void Execute(ref ManaTile tile)
        {
            if (Game.Ready && !Game.Paused)
                return;


        }

        public int2[] GetNeighbours(int2 coords, int2 max)
        {
            if (coords.x > 0 && coords.y > 0 && coords.x < max.x && coords.y < max.y)
            {
                var neighbours = new int2[8];
                neighbours[0] = new int2(coords.x + 1, coords.y);
                neighbours[0] = new int2(coords.x + 1, coords.y - 1);
                neighbours[0] = new int2(coords.x + 1, coords.y + 1);

                neighbours[0] = new int2(coords.x - 1, coords.y);
                neighbours[0] = new int2(coords.x - 1, coords.y - 1);
                neighbours[0] = new int2(coords.x - 1, coords.y + 1);

                neighbours[0] = new int2(coords.x, coords.y - 1);
                neighbours[0] = new int2(coords.x, coords.y + 1);
            }

            return new int2[0];
        }
    }
}