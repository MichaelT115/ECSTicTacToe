using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[DisableAutoCreation]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public class GridPrintSystem : JobComponentSystem
{
    EntityQuery query;

    private struct JobPrintGrid : IJob
    {
        [DeallocateOnJobCompletion]
        public NativeArray<NativeString4096> lines;
        public void Execute() { 
            Debug.Log(string.Join("\n", lines));
        }
    }

    private struct JobCreateRowString : IJobParallelFor
    {
        [ReadOnly] public ComponentDataFromEntity<OwnerComponent> componentDataFromEntity;
        private const string FORMAT = " {0}({1},{2}) ";
        [ReadOnly] public int width;
        [ReadOnly] public int height;
        [ReadOnly] public DynamicBuffer<GridCellData> gridBuffer;
        public NativeArray<NativeString4096> lines;

        public void Execute(int index)
        {
            StringBuilder sb = new StringBuilder(FORMAT.Length * width);
            int row = index;

            for (int i = row * width; i < (row + 1) * width; ++i)
            {
                int col = i % width;
                Team team = componentDataFromEntity[gridBuffer[i].entity].team;
                string teamString = team == Team.EMPTY ? "_" : team.ToString();
                sb.AppendFormat(FORMAT, teamString, col, row);
            }

            lines[index] = new NativeString4096(sb.ToString());
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
        JobCreateRowString createRowString = new JobCreateRowString()
        {
            componentDataFromEntity = GetComponentDataFromEntity<OwnerComponent>(true),
            width = width,
            height = height,
            gridBuffer = gridBuffer,
            lines = new NativeArray<NativeString4096>(height, Allocator.Persistent)
        };
        JobHandle jobHandle = createRowString.Schedule(height, 1);

        JobPrintGrid jobPrintGrid = new JobPrintGrid()
        {
            lines = createRowString.lines
        };

        return jobPrintGrid.Schedule(jobHandle);
    }
}
