using UnityEngine;
using Unity.Entities;

namespace Boids.Runtime
{
    
public struct School : IComponentData 
{
    public Entity Prefab;
    public int SpawnCount;
    public uint RandomSeed;
    public bool Initialized;
}

public class SchoolAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public int SpawnCount = 100;
    public uint RandomSeed = 100;
}

public class SchoolBaker : Baker<SchoolAuthoring>
{
    public override void Bake(SchoolAuthoring src)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        var prefab = GetEntity(src.Prefab, TransformUsageFlags.Dynamic);
        
        AddComponent(entity, new School()
        {
            Prefab = prefab,
            SpawnCount = src.SpawnCount,
            RandomSeed = src.RandomSeed,
            Initialized = false,
        });
    }
}

}