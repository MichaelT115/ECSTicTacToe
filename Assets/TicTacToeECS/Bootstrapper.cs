﻿using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    public int width = 3, height = 3;

    private EntityManager entityManager;

    public Mesh mesh;
    public Material material;

    void Start()
    {
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);

        entityManager = World.Active.EntityManager;

        // Create Game State Entity
        entityManager.CreateEntity(typeof(GameStateComponent));

        // Create Turn Controller Entity
        CreateTurnControllerEntity();

        // Create Player State Entities
        CreatePlayerEntities();

        // Create Grid
        CreateGrid();

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
        EntityArchetype tileArchetype = entityManager.CreateArchetype(typeof(OwnerComponent), typeof(Translation));
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
            entityManager.SetComponentData(tileEntity, new Translation() {
                Value = new float3(i % width, -(i / height), 0)
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