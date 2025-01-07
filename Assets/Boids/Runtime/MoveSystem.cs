using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{

public partial struct MoveSystem : ISystem
{
    ComponentLookup<Parameter> _ParamLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _ParamLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _ParamLookUp.Update(ref state);
        
        var dt = SystemAPI.Time.DeltaTime;
        
        foreach (var (fish, lt) in 
            SystemAPI.Query<
                RefRW<Fish>,
                RefRW<LocalTransform>>())
        {
            var param = _ParamLookUp.GetRefRO(fish.ValueRO.ParamEntity);
            var minSpeed = param.ValueRO.MinSpeed;
            var maxSpeed = param.ValueRO.MaxSpeed;
            
            fish.ValueRW.Velocity += fish.ValueRO.Acceleration * dt;
            var speed = math.length(fish.ValueRO.Velocity);
            speed = math.clamp(speed, minSpeed, maxSpeed);
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