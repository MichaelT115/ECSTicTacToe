using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Entities;

namespace BoardEvaluationTests
{
    [TestFixture]
    class BoardEvaluationColumnTests
    {
        static World World { get; set; }
        static EntityManager EntityManager { get; set; }

        [SetUp]
        public void Setup()
        {
            World = new World("Test World");
            EntityManager = World.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
            World.Dispose();
        }

        private static void UpdateSystems()
        {
            World.CreateSystem<BoardEvaluationColumnsSystem>().Update();
            World.GetOrCreateSystem<EndBoardEvaluationCommandBufferSystem>().Update();
        }


        private static void CreateBoardEntities(OwnerComponent[,] boardArray)
        {
            NativeList<GridCellData> gridCellData = new NativeList<GridCellData>(Allocator.Temp);
            int rowCount = boardArray.GetLength(0);
            int colCount = boardArray.GetLength(1);

            for (int row = 0; row < rowCount; ++row)
            {
                for (int col = 0; col < colCount; ++col)
                {
                    Entity tileEntity = EntityManager.CreateEntity();
                    EntityManager.AddComponentData(tileEntity, boardArray[row, col]);
                    gridCellData.Add(new GridCellData() { entity = tileEntity });

                }
            }

            Entity entity = EntityManager.CreateEntity();
            DynamicBuffer<GridCellData> grid = EntityManager.AddBuffer<GridCellData>(entity);
            grid.AddRange(gridCellData);

            gridCellData.Dispose();

            EntityManager.AddComponentData(entity, new GridDimensionsComponent()
            {
                columnCount = colCount,
                rowCount = rowCount
            });
        }

        [Test]
        public void FindMatchInColumn_SingleColumnBoard_FullColumnMatch()
        {
            var boardArray = new OwnerComponent[,]
            {
                { Team.X, },
                { Team.X, },
                { Team.X, },
            };            
            var matchSize = 3;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 1);
            Assert.AreEqual(matches[0].team, Team.X);
            Assert.AreEqual(matches[0].startIndex, 0);
            Assert.AreEqual(matches[0].endIndex, 2);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_SingleColumnBoard_MinMatchSizeLessThanColumnSize()
        {
            var boardArray = new OwnerComponent[,]
            {
                { Team.EMPTY, },
                { Team.X, },
                { Team.X, },
                { Team.X, },
                { Team.EMPTY },
            };

            var matchSize = 3;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 1);
            Assert.AreEqual(matches[0].team, Team.X);
            Assert.AreEqual(matches[0].startIndex, 1);
            Assert.AreEqual(matches[0].endIndex, 3);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_SingleColumnBoard_MatchSizeGreaterThanMin()
        {
            var boardArray = new OwnerComponent[,]
            {
                { Team.X, },
                { Team.X, },
                { Team.X, },
                { Team.X, },
                { Team.X, },
            };
            var matchSize = 1;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 1);
            Assert.AreEqual(matches[0].team, Team.X);
            Assert.AreEqual(matches[0].startIndex, 0);
            Assert.AreEqual(matches[0].endIndex, 4);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_SingleColumnBoard_MatchSizeSmallerThanMin()
        {
            var boardArray = new OwnerComponent[,]
            {
                { Team.X, },
                { Team.X,},
                { Team.EMPTY,},
            };
            var matchSize = 3;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 0);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_SingleColumnBoard_MatchSizeSmallerThanMinAndColumnSizeLessThanMatchSize()
        {
            var boardArray = new OwnerComponent[,]
            {
               { Team.X, }
            };           
            var matchSize = 3;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 0);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_SingleColumnBoard_EmptyBoard()
        {
            var boardArray = new OwnerComponent[,]
            {
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
                { Team.EMPTY, },
            };          
            var matchSize = 3;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 0);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_SingleColumnBoard_MultipleMatches()
        {
            var boardArray = new OwnerComponent[,]
            {
                { Team.X, },
                { Team.X, },
                { Team.EMPTY, },
                { Team.X, },
                { Team.X, },
                { Team.EMPTY, },
                { Team.X, },
                { Team.X, },
                { Team.X, },
                { Team.X, },
                { Team.EMPTY, },
                { Team.X, },
                { Team.X, },
            };
            var matchSize = 2;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 4);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_ZeroSizeBoard()
        {
            var boardArray = new OwnerComponent[,]
            {
            };            
            var matchSize = 3;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 0);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_MultiColumnBoard_NoMatches()
        {
            var boardArray = new OwnerComponent[,]
            {
                {   Team.X,     Team.X,     Team.X, },
                {   Team.X,     Team.X,     Team.X,},
                {   Team.EMPTY, Team.EMPTY, Team.EMPTY,},
            };
            
            var matchSize = 3;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 0);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_MultiColumnBoard_OneMatch()
        {
            var boardArray = new OwnerComponent[,]
            {
                {   Team.X,     Team.X, Team.X, },
                {   Team.X,     Team.X, Team.X, },
                {   Team.EMPTY, Team.X, Team.EMPTY, },
            };

            var matchSize = 3;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 1);
            Assert.AreEqual(matches[0].team, Team.X);
            Assert.AreEqual(matches[0].startIndex, 1);
            Assert.AreEqual(matches[0].endIndex, 7);

            matches.Dispose();
        }

        [Test]
        public void FindMatchInColumn_MultiColumnBoard_MultipleMatches()
        {
            var boardArray = new OwnerComponent[,]
            {
                {   Team.O,     Team.X, Team.X, },
                {   Team.O,     Team.X, Team.X, },
                {   Team.EMPTY, Team.X, Team.EMPTY, },
            };

            var matchSize = 2;

            CreateBoardEntities(boardArray);

            // Set Rules
            var rulesEntity = EntityManager.CreateEntity(typeof(MinMatchSizeRuleComponent));
            EntityManager.SetComponentData(rulesEntity, new MinMatchSizeRuleComponent() { minMatchSize = matchSize });

            UpdateSystems();

            var entityQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<MatchComponent>());
            var matches = entityQuery.ToComponentDataArray<MatchComponent>(Allocator.TempJob);

            Assert.AreEqual(matches.Length, 3);
            MatchComponent[] matchesArray = matches.ToArray();
            Assert.IsTrue(Array.Exists(matchesArray, match => match.team == Team.O && match.startIndex == 0 && match.endIndex == 3));
            Assert.IsTrue(Array.Exists(matchesArray, match => match.team == Team.X && match.startIndex == 1 && match.endIndex == 7));
            Assert.IsTrue(Array.Exists(matchesArray, match => match.team == Team.X && match.startIndex == 2 && match.endIndex == 5));

            matches.Dispose();
        }
    }
}
