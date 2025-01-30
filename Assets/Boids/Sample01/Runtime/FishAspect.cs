using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

namespace Boids.Sample01.Runtime
{

// This is purely an example of IAspect and is intentionally not used in this project.
public readonly partial struct FishAspect : IAspect
{
    readonly RefRW<Fish> _fish;
    readonly RefRW<LocalTransform> _localTransform;
    readonly DynamicBuffer<NeighborsEntityBufferElement> _neighborsEntityBuffer;
    
    public Fish Fish
    {
        get => _fish.ValueRO;
        set => _fish.ValueRW = value;
    }
    
    public float3 Velocity
    {
        get => _fish.ValueRO.Velocity;
        set => _fish.ValueRW.Velocity = value;
    }
    
    public float3 Acceleration 
    {
        get => _fish.ValueRO.Acceleration;
        set => _fish.ValueRW.Acceleration = value;
    }
    
    public Entity ParamEntity
    {
        get => _fish.ValueRO.ParamEntity;
    }
    
    public LocalTransform LocalTransform
    {
        get => _localTransform.ValueRO;
        set => _localTransform.ValueRW = value;
    }
    
    public DynamicBuffer<NeighborsEntityBufferElement> Neighbors => _neighborsEntityBuffer;
}
    
}
