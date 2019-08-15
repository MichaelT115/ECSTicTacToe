using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationDiagonalsSystem : JobComponentSystem
{
    EntityQuery gridQuery;
    EntityQuery gameStateQuery;
    EntityQuery gameRulesQuery;

    [BurstCompile]
    private struct FindDiagonalMatchesJob : IJob
    {
        [ReadOnly] public int startIndex;
        [ReadOnly] public int horizontalDirection;

        [ReadOnly] public int rowCount;
        [ReadOnly] public int colCount;
        [ReadOnly] public int minMatchSize;
        [ReadOnly] public NativeArray<OwnerComponent> board;

        [WriteOnly] public NativeList<MatchComponent> matchesFound;

        [BurstCompile]
        public void Execute()
        {
            int matchStartRow = GetRow(startIndex);
            int matchStartCol = GetCol(startIndex);

            int currentRow = matchStartRow + 1;
            int currentCol = matchStartCol + horizontalDirection;
            int matchSize = 1;

            Team currentTeam, matchTeam = board[GetIndex(matchStartRow, matchStartCol)];

            while (0 <= currentRow && currentRow < rowCount
                && 0 <= currentCol && currentCol < colCount)
            {
                currentTeam = board[GetIndex(currentRow, currentCol)];

                if (currentTeam != matchTeam)
                {
                    if (matchTeam != Team.EMPTY && matchSize >= minMatchSize)
                    {
                        int matchStartIndex = GetIndex(matchStartRow, matchStartCol);
                        int matchEndIndex = GetIndex(currentRow - 1, currentCol - horizontalDirection);
                        CreateMatch(matchStartIndex, matchEndIndex, matchTeam);
                    }

                    matchStartRow = currentRow;
                    matchStartCol = currentCol;
                    matchTeam = currentTeam;
                    matchSize = 0;
                }


                ++currentRow;
                currentCol += horizontalDirection;
                ++matchSize;
            }

            if (matchTeam != Team.EMPTY && matchSize >= minMatchSize)
            {
                int matchStartIndex = GetIndex(matchStartRow, matchStartCol);
                int matchEndIndex = GetIndex(currentRow - 1, currentCol - horizontalDirection);
                CreateMatch(matchStartIndex, matchEndIndex, matchTeam);
            }
        }

        private void CreateMatch(int matchStartIndex, int matchEndIndex, Team team)
        {
            matchesFound.Add(new MatchComponent()
            {
                team = team,
                startIndex = matchStartIndex,
                endIndex = matchEndIndex,
                matchType = MatchComponent.MatchType.DIAGONAL
            });
        }

        private int GetRow(int index)
        {
            return index / colCount;
        }

        private int GetCol(int index)
        {
            return index % colCount;
        }

        private int GetIndex(int row, int col)
        {
            return row * colCount + col % colCount;
        }
    }

    [BurstCompile]
    private struct CreateMatchEntitiesJob : IJob
    {
        [ReadOnly] public NativeArray<MatchComponent> matchesFound;
        [WriteOnly] public EntityCommandBuffer commandBuffer;

        public void Execute()
        {
            foreach (var match in matchesFound)
            {
                commandBuffer.AddComponent(commandBuffer.CreateEntity(), match);
            }
        }
    }

    private struct DeallocateNativeArray<T> : IJob
        where T : struct
    {
        [DeallocateOnJobCompletion]
        public NativeArray<T> nativeArray;

        public void Execute() { }
    }


    protected override void OnCreate()
    {
        gridQuery = GetEntityQuery(typeof(GridCellData));
        gameStateQuery = GetEntityQuery(typeof(GameStateComponent));
        gameRulesQuery = GetEntityQuery(typeof(MinMatchSizeRuleComponent));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        Entity gridEntity = gridQuery.GetSingletonEntity();
        GridDimensionsComponent gridDimensions = EntityManager.GetComponentData<GridDimensionsComponent>(gridEntity);

        EndBoardEvaluationCommandBufferSystem commandBufferSystem = World.GetOrCreateSystem<EndBoardEvaluationCommandBufferSystem>();

        int minMatchSize = gameRulesQuery.GetSingleton<MinMatchSizeRuleComponent>();

        DynamicBuffer<GridCellData> buffer = EntityManager.GetBuffer<GridCellData>(gridEntity);
        NativeArray<OwnerComponent> ownerGrid = new NativeArray<OwnerComponent>(buffer.Length, Allocator.TempJob);

        if (buffer.Length == 0)
        {
            return inputDeps;
        }

        for (int i = 0; i < ownerGrid.Length; ++i)
        {
            ownerGrid[i] = EntityManager.GetComponentData<OwnerComponent>(buffer[i].entity);
        }

        int columnCount = gridDimensions.columnCount;
        int rowCount = gridDimensions.rowCount;
        int lastIndexInBoard = columnCount * rowCount;

        var jobHandles = new NativeList<JobHandle>(Allocator.TempJob);

        // Diagonal Checks starting from top row.
        for (int startIndex = 0; startIndex < columnCount; ++startIndex)
        {
            jobHandles.Add(StartFindDiagonalMatchesJob(
                ownerGrid: ownerGrid,
                minMatchSize: minMatchSize,
                columnCount: columnCount,
                rowCount: rowCount,
                startIndex: startIndex,
                horizontalDirection: 1,
                commandBufferSystem: commandBufferSystem));

            jobHandles.Add(StartFindDiagonalMatchesJob(
                ownerGrid: ownerGrid,
                minMatchSize: minMatchSize,
                columnCount: columnCount,
                rowCount: rowCount,
                startIndex: startIndex,
                horizontalDirection: -1,
                commandBufferSystem: commandBufferSystem));
        }

        // Diagonal Checks starting from Left Column
        int secondIndexInLeftColumn = columnCount;
        for (int startIndex = secondIndexInLeftColumn; startIndex < lastIndexInBoard; startIndex += rowCount)
        {
            jobHandles.Add(StartFindDiagonalMatchesJob(
                ownerGrid: ownerGrid,
                minMatchSize: minMatchSize,
                columnCount: columnCount,
                rowCount: rowCount,
                startIndex: startIndex,
                horizontalDirection: 1,
                commandBufferSystem: commandBufferSystem));
        }

        // Diagonal Checks starting from Right Column
        int secondIndexInRightColumn = columnCount + columnCount - 1;
        for (int startIndex = secondIndexInRightColumn; startIndex < lastIndexInBoard; startIndex += rowCount)
        {
            jobHandles.Add(StartFindDiagonalMatchesJob(
                ownerGrid: ownerGrid,
                minMatchSize: minMatchSize,
                columnCount: columnCount,
                rowCount: rowCount,
                startIndex: startIndex,
                horizontalDirection: -1,
                commandBufferSystem: commandBufferSystem));
        }

        new DeallocateNativeArray<OwnerComponent>()
        {
            nativeArray = ownerGrid
        }.Schedule(JobHandle.CombineDependencies(jobHandles));


        jobHandles.Dispose();

        return inputDeps;
    }

    private static JobHandle StartFindDiagonalMatchesJob(NativeArray<OwnerComponent> ownerGrid, int minMatchSize, int columnCount, int rowCount, int startIndex, int horizontalDirection, EntityCommandBufferSystem commandBufferSystem)
    {
        NativeList<MatchComponent> matchesFound = new NativeList<MatchComponent>(Allocator.TempJob);
        var findDiagonalMatchesJobHandle = new FindDiagonalMatchesJob()
        {
            startIndex = startIndex,

            colCount = columnCount,
            rowCount = rowCount,

            horizontalDirection = horizontalDirection,
            minMatchSize = minMatchSize,
            board = ownerGrid,
            matchesFound = matchesFound
        }.Schedule();

        var createMatchEntitiesJobHandle = new CreateMatchEntitiesJob()
        {
            matchesFound = matchesFound.AsDeferredJobArray(),
            commandBuffer = commandBufferSystem.CreateCommandBuffer()
        }.Schedule(findDiagonalMatchesJobHandle);

        matchesFound.Dispose(createMatchEntitiesJobHandle);

        commandBufferSystem.AddJobHandleForProducer(createMatchEntitiesJobHandle);

        return createMatchEntitiesJobHandle;
    }
}
