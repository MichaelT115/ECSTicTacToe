using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[DisableAutoCreation]
public class RandomizeBoardSystem : JobComponentSystem
{
    private EntityQuery entityQuery;

    private struct RandomizeBoardJob : IJobForEach_C<OwnerComponent>
    {
        public Unity.Mathematics.Random random;

        public void Execute(ref OwnerComponent teamComponent) =>
            teamComponent.team = (Team)random.NextInt(0, (int)Team.O + 1);
    }

    protected override void OnCreate()
    {
        entityQuery = GetEntityQuery(typeof(OwnerComponent));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) => new RandomizeBoardJob()
    {
        random = new Unity.Mathematics.Random((uint)Random.Range(1, int.MaxValue))
    }.Schedule(entityQuery);
}