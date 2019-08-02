using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    public int width = 3, height = 3, sequenceSize = 3;

    private EntityManager entityManager;

    public Mesh mesh;

    public Material defaultMaterial, xMaterial, yMaterial;

    void Start()
    {
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);

        entityManager = World.Active.EntityManager;

        // Create Game State Entity
        Entity gameStateEntity = entityManager.CreateEntity(typeof(GameStateComponent));

        // Create Turn Controller Entity
        CreateTurnControllerEntity();

        // Create Player State Entities
        CreatePlayerEntities();

        // Create Grid
        CreateGrid();

        Camera camera = Camera.main;
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        camera.transform.position = new Vector3(halfWidth, halfHeight, -10);
        camera.orthographicSize = Mathf.Max(halfHeight, halfWidth / camera.aspect);

        World.Active.GetOrCreateSystem<StartTurnSystem>().Update();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomizeGrid();
        }
    }

    private void CreateTurnControllerEntity()
    {
        Entity turnControllerEntity = entityManager.CreateEntity(typeof(CurrentPlayerIndexComponent));
        entityManager.AddBuffer<PlayerListElement>(turnControllerEntity);
    }

    private void CreatePlayerEntities()
    {
        World.Active.CreateSystem<CreatePlayersSystem>().Update();
    }

    private void CreateGrid()
    {
        EntityArchetype tileArchetype = entityManager.CreateArchetype(typeof(OwnerComponent), typeof(Translation), typeof(RectangleComponent));
        Entity gridEntity = entityManager.CreateEntity();

        entityManager.AddComponentData(gridEntity, new GridDimensionsComponent()
        {
            columnCount = width,
            rowCount = height
        });

        defaultMaterial.color = Color.white;

        NativeArray<GridCellData> gridComponents = new NativeArray<GridCellData>(width * height, Allocator.Temp);
        for (int i = 0; i < width * height; ++i)
        {
            Entity tileEntity = entityManager.CreateEntity(tileArchetype);
            entityManager.SetComponentData(tileEntity, new OwnerComponent() { team = 0 });
            entityManager.SetComponentData(tileEntity, new Translation() {
                Value = new float3(i % width + 0.5f, (i / width) + 0.5f, 0)
            });
            entityManager.SetComponentData(tileEntity, new RectangleComponent()
            {
                xMin = (i % width),
                xMax = (i % width) + 1,
                yMin = (i / width),
                yMax = (i / width) + 1
            });

            entityManager.AddComponent(tileEntity, ComponentType.ReadOnly<Rotation>());
            entityManager.AddComponentData(tileEntity, new NonUniformScale() {
                Value = new float3(0.9f, 0.9f, 0.9f)
            });
            entityManager.AddComponent(tileEntity, ComponentType.ReadOnly<LocalToWorld>());
            entityManager.AddComponent(tileEntity, ComponentType.ReadOnly<Rotation>());

            entityManager.AddSharedComponentData(tileEntity, new RenderMesh()
            {
                mesh = mesh,
                material = defaultMaterial,
            });

            gridComponents[i] = new GridCellData() { entity = tileEntity };
        }

        DynamicBuffer<GridCellData> gridBuffer = entityManager.AddBuffer<GridCellData>(gridEntity);
        gridBuffer.AddRange(gridComponents);
        gridComponents.Dispose();
    }

    [ContextMenu("Randomize")]
    public void RandomizeGrid()
    {
        World world = World.Active;
        world.GetOrCreateSystem<RandomizeBoardSystem>().Update();
        world.GetOrCreateSystem<GridPrintSystem>().Update();
    }
}


public struct RectangleComponent : IComponentData
{
    public float xMin, yMin, xMax, yMax;
}

public static class CollisionHelper
{
    [BurstCompile]
    public static bool Intersect(float2 point,  RectangleComponent rectangle)
    {
        return (rectangle.xMin <= point.x && point.x <= rectangle.xMax)
            && (rectangle.yMin <= point.y && point.y <= rectangle.yMax);
    }
}