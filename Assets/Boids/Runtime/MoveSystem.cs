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
            
            var v = fish.ValueRO.Velocity;
            v += fish.ValueRO.Acceleration * dt;
            var speed = math.length(v);
            speed = math.clamp(speed, param.MinSpeed, param.MaxSpeed);
            var dir = math.normalize(v);
            v = dir * speed;
            fish.ValueRW.Velocity = v;
            
            fish.ValueRW.Acceleration = 0f;
            
            var pos = lt.ValueRO.Position;
            pos += fish.ValueRO.Velocity * dt;
            lt.ValueRW.Position = pos;
            
            var up = math.up();
            lt.ValueRW.Rotation = quaternion.LookRotationSafe(dir, up);
        }
    }
}

}