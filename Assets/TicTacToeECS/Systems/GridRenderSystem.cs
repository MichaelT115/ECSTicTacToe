using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public class GridRenderSystem : ComponentSystem
{
    Mesh mesh;
    Material[] materials;
    EntityQuery query;

    protected override void OnCreate()
    {
        Bootstrapper testing = Object.FindObjectOfType<Bootstrapper>();
        Material material = testing.material;
        mesh = testing.mesh;
        query = GetEntityQuery(typeof(OwnerComponent), typeof(Translation));
        
        materials = new Material[3];
        materials[0] = new Material(material);
        materials[1] = new Material(material);
        materials[2] = new Material(material);

        materials[1].color = Color.red;
        materials[2].color = Color.yellow;
    }

    protected override void OnUpdate()
    {
        var translations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
        var teamComponents = query.ToComponentDataArray<OwnerComponent>(Allocator.TempJob);

        for (int i = 0; i < translations.Length; ++i)
        {
            var translation = translations[i];
            Vector3 position = new Vector3(translation.Value.x, translation.Value.y);
            Graphics.DrawMesh(mesh, position, Quaternion.identity, materials[(int)teamComponents[i].team], 0);
        }

        translations.Dispose();
        teamComponents.Dispose();
    }
}