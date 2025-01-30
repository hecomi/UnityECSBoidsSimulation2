using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Boids.Sample03.Runtime
{
    
public static class FishConfig
{
    public const int NeighborsEntityBufferMaxSize = 8;
}

public struct Fish : IComponentData
{
    public float3 Velocity;
    public float3 Acceleration;
    public Entity ParamEntity;
}

public struct FishJobData : IComponentData
{
    public float3 Position;
    public float3 Velocity;
}

[InternalBufferCapacity(FishConfig.NeighborsEntityBufferMaxSize)]
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
        var v = UnityEngine.Random.insideUnitSphere;
        
        AddComponent(entity, new Fish()
        {
            Velocity = v,
            Acceleration = 0f,
            ParamEntity = Entity.Null,
        });
        
        AddComponent(entity, new FishJobData()
        {
            Position = src.transform.position,
            Velocity = v,
        });
        
        AddBuffer<NeighborsEntityBufferElement>(entity);
    }
}

}