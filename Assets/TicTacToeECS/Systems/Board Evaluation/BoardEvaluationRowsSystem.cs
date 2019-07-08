using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationRowsSystem : JobComponentSystem
{
    EntityQuery query;

    private struct CheckRowJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataFromEntity<OwnerComponent> componentDataFromEntity;
        [ReadOnly] public int width;
        [ReadOnly] public DynamicBuffer<GridCellData> gridBuffer;
        [WriteOnly] public NativeArray<Team> winner;

        public void Execute(int row)
        {
            int firstInRowIndex = row * width;
            int lastInRowIndex = (row + 1) * width - 1;

            Team previousTeam = componentDataFromEntity[gridBuffer[firstInRowIndex].entity].team;

            if (previousTeam == Team.EMPTY)
                return;

            for (int i = firstInRowIndex + 1; i <= lastInRowIndex; ++i)
            {
                Team team = componentDataFromEntity[gridBuffer[i].entity].team;

                // Empty Space
                if (team == Team.EMPTY)
                    return;

                // Does not match previous cell.
                if (team != previousTeam)
                    return;
            }

            winner[row] = previousTeam;
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

        CheckRowJob checkRowJob = new CheckRowJob()
        {
            componentDataFromEntity = GetComponentDataFromEntity<OwnerComponent>(true),
            width = width,
            gridBuffer = gridBuffer,
            winner = new NativeArray<Team>(gridBuffer.Length, Allocator.TempJob)
        };
        JobHandle jobHandle = checkRowJob.Schedule(height, 1, inputDeps);
        jobHandle.Complete();

        // Find Winner
        Team winner = Team.EMPTY;
        for (int i = 0; i < checkRowJob.winner.Length; ++i)
        {
            if (checkRowJob.winner[i] != Team.EMPTY)
            {
                winner = checkRowJob.winner[i];
                break;
            }
        }
        checkRowJob.winner.Dispose();

        if (winner != Team.EMPTY)
        {
            Debug.Log("Winner: " + winner);
        }


        return jobHandle;
    }
}
