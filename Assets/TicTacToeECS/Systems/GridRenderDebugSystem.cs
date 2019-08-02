using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[DisableAutoCreation]
public class GridRenderDebugSystem : ComponentSystem
{
    private EntityQuery highlightedRectanglesQuery;
    private EntityQuery nonhighlightedRectanglesQuery;

    protected override void OnCreate()
    {
        highlightedRectanglesQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[]
             {
                typeof(RectangleComponent),
                typeof(FocusComponent)
             }
        });

        nonhighlightedRectanglesQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[]
            {
                typeof(RectangleComponent)
            },
            None = new ComponentType[]
            {
                typeof(FocusComponent)
            }
        });
    }

    protected override void OnUpdate()
    {
        var nonHighlightedRectangles = nonhighlightedRectanglesQuery.ToComponentDataArray<RectangleComponent>(Allocator.TempJob);
        foreach (var rectangleComponent in nonHighlightedRectangles)
        {
            float xMin = rectangleComponent.xMin;
            float yMin = rectangleComponent.yMin;
            float xMax = rectangleComponent.xMax;
            float yMax = rectangleComponent.yMax;

            Debug.DrawLine(new Vector3(xMin, yMin), new Vector3(xMax, yMax), Color.black, 0, false);
            Debug.DrawLine(new Vector3(xMin, yMax), new Vector3(xMax, yMin), Color.black, 0, false);

            Debug.DrawLine(new Vector3(xMin, yMin), new Vector3(xMax, yMin), Color.gray, 0, false);
            Debug.DrawLine(new Vector3(xMin, yMax), new Vector3(xMax, yMax), Color.gray, 0, false);
            Debug.DrawLine(new Vector3(xMin, yMin), new Vector3(xMin, yMax), Color.gray, 0, false);
            Debug.DrawLine(new Vector3(xMax, yMin), new Vector3(xMax, yMax), Color.gray, 0, false);
        }
        nonHighlightedRectangles.Dispose();

        var highlightedRectangles= highlightedRectanglesQuery.ToComponentDataArray<RectangleComponent>(Allocator.TempJob);
        foreach (var rectangleComponent in highlightedRectangles)
        {
            float xMin = rectangleComponent.xMin;
            float yMin = rectangleComponent.yMin;
            float xMax = rectangleComponent.xMax;
            float yMax = rectangleComponent.yMax;

            Debug.DrawLine(new Vector3(xMin, yMin), new Vector3(xMax, yMax), Color.green, 0, false);
            Debug.DrawLine(new Vector3(xMin, yMax), new Vector3(xMax, yMin), Color.green, 0, false);

            Debug.DrawLine(new Vector3(xMin, yMin), new Vector3(xMax, yMin), Color.gray, 0, false);
            Debug.DrawLine(new Vector3(xMin, yMax), new Vector3(xMax, yMax), Color.gray, 0, false);
            Debug.DrawLine(new Vector3(xMin, yMin), new Vector3(xMin, yMax), Color.gray, 0, false);
            Debug.DrawLine(new Vector3(xMax, yMin), new Vector3(xMax, yMax), Color.gray, 0, false);
        }
        highlightedRectangles.Dispose();
    }
}