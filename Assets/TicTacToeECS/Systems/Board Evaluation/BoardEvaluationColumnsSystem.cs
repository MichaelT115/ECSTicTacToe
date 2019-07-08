using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationColumnsSystem : JobComponentSystem
{
    EntityQuery query;

    private struct CheckColumnJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataFromEntity<OwnerComponent> componentDataFromEntity;
        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public DynamicBuffer<GridCellData> gridBuffer;
        [WriteOnly] public NativeArray<Team> winner;

        public void Execute(int column)
        {
            int firstInColumnIndex = column;
            int lastInColumnIndex = column + (height - 1) * width;

            Team previousTeam = componentDataFromEntity[gridBuffer[firstInColumnIndex].entity].team;

            if (previousTeam == Team.EMPTY)
                return;

            for (int i = firstInColumnIndex; i <= lastInColumnIndex; i += width)
            {
                Team team = componentDataFromEntity[gridBuffer[i].entity].team;

                // Empty Space
                if (team == Team.EMPTY)
                    return;

                // Does not match previous cell.
                if (team != previousTeam)
                    return;
            }

            winner[column] = previousTeam;
        }
    }

    protected override void OnCreate()
    {
        query = GetEntityQuery(typeof(GridCellData));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityManager entityManager = World.Active.EntityManager;
        Entity gridEntity = query.GetSingletonEntity();
        GridDimensionsComponent gridDimensions = entityManager.GetComponentData<GridDimensionsComponent>(gridEntity);
        int width = gridDimensions.columnCount;
        int height = gridDimensions.rowCount;
        DynamicBuffer<GridCellData> gridBuffer = entityManager.GetBuffer<GridCellData>(gridEntity);

        CheckColumnJob checkColumnJob = new CheckColumnJob()
        {
            componentDataFromEntity = GetComponentDataFromEntity<OwnerComponent>(true),
            width = width,
            height = height,
            gridBuffer = gridBuffer,
            winner = new NativeArray<Team>(gridBuffer.Length, Allocator.TempJob)
        };
        JobHandle jobHandle = checkColumnJob.Schedule(height, 1, inputDeps);
        jobHandle.Complete();

        // Find Winner
        Team winner = Team.EMPTY;
        for (int i = 0; i < checkColumnJob.winner.Length; ++i)
        {
            if (checkColumnJob.winner[i] != Team.EMPTY)
            {
                winner = checkColumnJob.winner[i];
                break;
            }
        }
        checkColumnJob.winner.Dispose();

        if (winner != Team.EMPTY)
        {
            Debug.Log("Winner: " + winner);
        }

        return jobHandle;
    }
}