using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public int width = 3, height = 3;

    private EntityArchetype tileArchetype;

    public Mesh mesh;
    public Material material;

    RandomizeBoardSystem randomizeBoardSystem;

    void Start()
    {
        EntityManager entityManager = World.Active.EntityManager;

        tileArchetype = entityManager.CreateArchetype(typeof(OwnerComponent), typeof(Translation));

        CreateGrid(entityManager);

        randomizeBoardSystem = World.Active.CreateSystem<RandomizeBoardSystem>();

        Entity playerEntity = entityManager.CreateEntity(typeof(PlayerTeamComponent), typeof(HasTurnComponent));
        entityManager.SetComponentData(playerEntity, new PlayerTeamComponent() { team = Team.X });

        Entity gameStateEntity = entityManager.CreateEntity(typeof(GameStateComponent));
    }
    
    [ContextMenu("Randomize")]
    public void RandomizeGrid()
    {
        randomizeBoardSystem.Update();
        var updateGroup = World.Active.GetOrCreateSystem<BoardEvaluationUpdateGroup>();

        updateGroup.Update();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomizeGrid();
        }
    }

    private void CreateGrid(EntityManager entityManager)
    {
        Entity gridEntity = entityManager.CreateEntity();

        entityManager.AddComponentData(gridEntity, new GridDimensionsComponent()
        {
            columnCount = width,
            rowCount = height
        });
        NativeArray<GridCellData> gridComponents = new NativeArray<GridCellData>(width * height, Allocator.Temp);
        for (int i = 0; i < width * height; ++i)
        {
            Entity tileEntity = entityManager.CreateEntity(tileArchetype);
            entityManager.SetComponentData(tileEntity, new OwnerComponent() { team = 0 });
            entityManager.SetComponentData(tileEntity, new Translation() { Value = new float3(i % width, i / height, 0) });

            gridComponents[i] = new GridCellData() { entity = tileEntity };
        }

        DynamicBuffer<GridCellData> gridBuffer = entityManager.AddBuffer<GridCellData>(gridEntity);
        gridBuffer.AddRange(gridComponents);
    }
}
