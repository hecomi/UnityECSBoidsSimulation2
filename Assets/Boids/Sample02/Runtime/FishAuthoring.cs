using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Boids.Sample02.Runtime
{

public struct Fish : IComponentData
{
    public float3 Velocity;
    public float3 Acceleration;
    public Entity ParamEntity;
}

[InternalBufferCapacity(8)]
public struct NeighborsEntityBufferElement : IBufferElementData
{
    public Entity Entity;
}

public class FishAuthoring : MonoBehaviour
{
}

public class FishBaker : Baker<FishAuthoring>
{
    public override void Bake(FishAuthoring src)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        
        AddComponent(entity, new Fish()
        {
            Velocity = UnityEngine.Random.insideUnitSphere,
            Acceleration = 0f,
            ParamEntity = Entity.Null,
        });
        
        AddBuffer<NeighborsEntityBufferElement>(entity);
    }
}

}