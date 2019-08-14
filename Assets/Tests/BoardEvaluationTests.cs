using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

class BoardEvaluationTests
{
    class BoardEvaluationColumnTests
    {
        static EntityManager EntityManager { get; set; }

        [SetUp]
        void Setup()
        {
            EntityManager = World.Active.EntityManager;
        }

        [TearDown]
        void TearDown()
        {
            World.Active.Dispose();
        }

        [Test]
        void FindMatchInColumn_SingleColumnBoard_FullColumnMatch()
        {
            var boardArray = new OwnerComponent[]
            {
                Team.X,
                Team.X,
                Team.X,
            };

            var colCount = 1;
            var rowCount = 3;
            var matchSize = 3;

            CreateBoardEntities(boardArray, colCount, rowCount);

           var system =  World.Active.CreateSystem<BoardEvaluationColumnsSystem>();

            system.Update();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.Temp);

            Assert.AreEqual(matches.Length, 1);
            Assert.AreEqual(matches[0].team, Team.X);
            Assert.AreEqual(matches[0].startIndex, 0);
            Assert.AreEqual(matches[0].endIndex, 2);

            board.Dispose();
            matchesQueue.Dispose();
            matches.Dispose();
        }

        private static void CreateBoardEntities(OwnerComponent[] boardArray, int colCount, int rowCount)
        {
            NativeList<GridCellData> gridCellData = new NativeList<GridCellData>(Allocator.Temp);

            for (int row = 0; row < rowCount; ++row)
            {
                for (int col = 0; col < colCount; ++col)
                {
                    Entity tileEntity = EntityManager.CreateEntity();
                    EntityManager.AddComponentData(tileEntity, boardArray[row * colCount + col]);
                    gridCellData.Add(new GridCellData() { entity = tileEntity });

                }
            }

            Entity entity = EntityManager.CreateEntity();
            DynamicBuffer<GridCellData> grid = EntityManager.AddBuffer<GridCellData>(entity);
            grid.AddRange(gridCellData);

            gridCellData.Dispose();
        }

        [Test]
        public void Test_FindMatchInColumn_SingleColumnBoard_MatchSizeLessThanColumnSize()
        {
            var boardArray = new OwnerComponent[]
            {
                Team.EMPTY,
                Team.X,
                Team.X,
                Team.X,
                Team.EMPTY
            };

            var colCount = 1;
            var rowCount = 5;
            var matchSize = 3;

            NativeQueue<MatchComponent> matchesQueue = new NativeQueue<MatchComponent>(Allocator.TempJob);
            NativeArray<OwnerComponent> board = new NativeArray<OwnerComponent>(boardArray, Allocator.TempJob);
            NativeList<MatchComponent> matches = new NativeList<MatchComponent>(matchesQueue.Count, Allocator.TempJob);

            new FindMatchesInColumn()
            {
                rowCount = rowCount,
                colCount = colCount,
                matchSize = matchSize,
                board = board,

                matchesFound = matchesQueue.AsParallelWriter()
            }.Run(colCount);
            new ConvertNativeQueueToNativeList<MatchComponent>()
            {
                queue = matchesQueue,
                list = matches
            }.Run();

            Assert.AreEqual(matches.Length, 1);
            Assert.AreEqual(matches[0].team, Team.X);
            Assert.AreEqual(matches[0].startIndex, 1);
            Assert.AreEqual(matches[0].endIndex, 3);

            board.Dispose();
            matchesQueue.Dispose();
            matches.Dispose();
        }

        [Test]
        public void Test_FindMatchInColumn_SingleColumnBoard_MatchSizeGreaterThanMin()
        {
            var boardArray = new OwnerComponent[]
            {
                Team.X,
                Team.X,
                Team.X,
                Team.X,
                Team.X,
            };

            var colCount = 1;
            var rowCount = 5;
            var matchSize = 1;

            NativeQueue<MatchComponent> matchesQueue = new NativeQueue<MatchComponent>(Allocator.TempJob);
            NativeArray<OwnerComponent> board = new NativeArray<OwnerComponent>(boardArray, Allocator.TempJob);
            NativeList<MatchComponent> matches = new NativeList<MatchComponent>(matchesQueue.Count, Allocator.TempJob);

            new FindMatchesInColumn()
            {
                rowCount = rowCount,
                colCount = colCount,
                matchSize = matchSize,
                board = board,

                matchesFound = matchesQueue.AsParallelWriter()
            }.Run(colCount);
            new ConvertNativeQueueToNativeList<MatchComponent>()
            {
                queue = matchesQueue,
                list = matches
            }.Run();

            Assert.AreEqual(matches.Length, 1);
            Assert.AreEqual(matches[0].team, Team.X);
            Assert.AreEqual(matches[0].startIndex, 0);
            Assert.AreEqual(matches[0].endIndex, 4);

            board.Dispose();
            matchesQueue.Dispose();
            matches.Dispose();
        }
    }
}
