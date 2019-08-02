using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationDiagonalsSystem : JobComponentSystem
{
    EndBoardEvaluationCommandBufferSystem commandBufferSystem;

    EntityQuery gridQuery;

    [BurstCompile]
    private struct CheckDiagonalMatchesJob : IJob
    {
        [ReadOnly] public int startIndex;
      
        [ReadOnly] public GridDimensionsComponent gridDimensions;
        [ReadOnly] public int horizontalDirection;
        [ReadOnly] public int minMatchSize;
        [ReadOnly] public NativeArray<Team> teamArray;

        [WriteOnly] public NativeList<MatchComponent> matchesFound;

        public void Execute()
        {
            int colCount = gridDimensions.columnCount;
            int rowCount = gridDimensions.rowCount;

            int prevIndex = startIndex;
            int currentIndex = startIndex + horizontalDirection + rowCount;

            int matchStartIndex = prevIndex;
            int matchSize = 1;

            Team currentTeam,
               previousTeam = teamArray[prevIndex];

            while (GetRow(currentIndex) < rowCount && GetCol(currentIndex) < rowCount)
            {
                currentTeam = teamArray[currentIndex];

                if (currentTeam != previousTeam)
                {
                    if (previousTeam != Team.EMPTY && matchSize >= minMatchSize)
                    {
                        CreateMatch(prevIndex, matchStartIndex, previousTeam);
                    }

                    matchStartIndex = currentIndex;
                    previousTeam = currentTeam;
                    matchSize = 0;
                }


                prevIndex = currentIndex;
                currentIndex += rowCount + horizontalDirection;
                ++matchSize;
            }

            if (previousTeam != Team.EMPTY && matchSize >= minMatchSize)
            {
                CreateMatch(prevIndex, matchStartIndex, previousTeam);
            }
        }

        private void CreateMatch(int prevIndex, int matchStartIndex, Team previousTeam)
        {
            matchesFound.Add(new MatchComponent()
            {
                team = previousTeam,
                startIndex = matchStartIndex,
                endIndex = prevIndex,
                matchType = MatchComponent.MatchType.DIAGONAL
            });
        }

        private int GetRow(int index)
        {
            return index / gridDimensions.columnCount;
        }

        private int GetCol(int index)
        {
            return index % gridDimensions.columnCount;
        }

        private int GetIndex(int row, int col)
        {
            return row * gridDimensions.columnCount + col % gridDimensions.columnCount;
        }
    }

    [BurstCompile]
    private struct GetTeamArrayJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataFromEntity<OwnerComponent> componentDataFromEntity;
        [ReadOnly] public NativeArray<GridCellData> gridDataArray;
        [WriteOnly] public NativeArray<Team> teamArray;

        public void Execute(int index)
        {
            teamArray[index] = componentDataFromEntity[gridDataArray[index].entity].team;
        }
    }

    private struct CreateMatchEntitiesJob : IJob
    {
        [ReadOnly] public NativeArray<MatchComponent> matchesFound;
        [WriteOnly] public EntityCommandBuffer commandBuffer;

        public void Execute()
        {
            foreach(var match in matchesFound)
            {
                commandBuffer.AddComponent(commandBuffer.CreateEntity(), match);
            }
        }
    }

    private struct DisposeTeamArray : IJob
    {
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Team> teamArray;

        public void Execute() { }
    }

    protected override void OnCreate()
    {
        gridQuery = GetEntityQuery(typeof(GridCellData));

        commandBufferSystem = World.GetOrCreateSystem<EndBoardEvaluationCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entity gridEntity = gridQuery.GetSingletonEntity();
        GridDimensionsComponent gridDimensions = EntityManager.GetComponentData<GridDimensionsComponent>(gridEntity);
        NativeArray<GridCellData> gridDataArray = EntityManager.GetBuffer<GridCellData>(gridEntity).AsNativeArray();
        NativeArray<Team> teamArray = new NativeArray<Team>(gridDataArray.Length, Allocator.TempJob);

        JobHandle getTeamArrayJobHandle = new GetTeamArrayJob()
        {
            componentDataFromEntity = GetComponentDataFromEntity<OwnerComponent>(true),
            gridDataArray = gridDataArray,
            teamArray = teamArray
        }.Schedule(gridDataArray.Length, 1);

        var jobs = CreateCheckDiagonalJobs(gridDimensions, teamArray);
        var jobHandles = new NativeList<JobHandle>(jobs.Count, Allocator.TempJob);
        foreach (var job in jobs)
        {
            JobHandle jobHandle = job.Schedule(getTeamArrayJobHandle);

            jobHandle = new CreateMatchEntitiesJob()
            {
                matchesFound = job.matchesFound.AsDeferredJobArray(),
                commandBuffer = commandBufferSystem.CreateCommandBuffer()
            }.Schedule(jobHandle);

            job.matchesFound.Dispose(jobHandle);


            jobHandles.Add(jobHandle);
        }

        var combinedJobHandles = JobHandle.CombineDependencies(jobHandles);

        jobHandles.Dispose();

        commandBufferSystem.AddJobHandleForProducer(combinedJobHandles);

        new DisposeTeamArray()
        {
            teamArray = teamArray
        }.Schedule(combinedJobHandles);

        return getTeamArrayJobHandle;
    }

    private static List<CheckDiagonalMatchesJob> CreateCheckDiagonalJobs(GridDimensionsComponent gridDimensions, NativeArray<Team> teamArray)
    {
        int columnCount = gridDimensions.columnCount;
        int rowCount = gridDimensions.rowCount;
        int totalCells = rowCount * columnCount;

        var jobs = new List<CheckDiagonalMatchesJob>();
        for (int i = 0; i < columnCount; ++i)
        {
            jobs.Add(new CheckDiagonalMatchesJob()
            {
                startIndex = i,

                gridDimensions = gridDimensions,

                horizontalDirection = 1,
                minMatchSize = 1,
                teamArray = teamArray,

                matchesFound = new NativeList<MatchComponent>(Allocator.TempJob)
            });

            jobs.Add(new CheckDiagonalMatchesJob()
            {
                startIndex = i,

                gridDimensions = gridDimensions,

                horizontalDirection = -1,
                minMatchSize = 1,
                teamArray = teamArray,

                matchesFound = new NativeList<MatchComponent>(Allocator.TempJob)
            });
        }

        // Left Column
        for (int i = columnCount + 1; i < totalCells; i += rowCount)
        {
            jobs.Add(new CheckDiagonalMatchesJob()
            {
                startIndex = i,

                gridDimensions = gridDimensions,

                horizontalDirection = 1,
                minMatchSize = 1,
                teamArray = teamArray,

                matchesFound = new NativeList<MatchComponent>(Allocator.TempJob)
            });
        }

        // Right Column
        for (int i = columnCount * 2 - 1; i < totalCells; i += rowCount)
        {
            jobs.Add(new CheckDiagonalMatchesJob()
            {
                startIndex = i,

                gridDimensions = gridDimensions,

                horizontalDirection = -1,
                minMatchSize = 1,
                teamArray = teamArray,

                matchesFound = new NativeList<MatchComponent>(Allocator.TempJob)
            });
        }

        return jobs;
    }
}
