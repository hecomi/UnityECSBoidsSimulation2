using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Runtime
{

public partial struct MoveSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        
        var dt = SystemAPI.Time.DeltaTime;
        
        foreach (var (fish, lt) in 
            SystemAPI.Query<
                RefRW<Fish>,
                RefRW<LocalTransform>>())
        {
            var param = _paramLookUp[fish.ValueRO.ParamEntity];
            
            fish.ValueRW.Velocity += fish.ValueRO.Acceleration * dt;
            var speed = math.length(fish.ValueRO.Velocity);
            speed = math.clamp(speed, param.MinSpeed, param.MaxSpeed);
            var dir = math.normalize(fish.ValueRO.Velocity);
            fish.ValueRW.Velocity = dir * speed;
            fish.ValueRW.Acceleration = 0f;
            
            lt.ValueRW.Position += fish.ValueRO.Velocity * dt;
            var up = math.up();
            lt.ValueRW.Rotation = quaternion.LookRotationSafe(dir, up);
        }
    }
}

}