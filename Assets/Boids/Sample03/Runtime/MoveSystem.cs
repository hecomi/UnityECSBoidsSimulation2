using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Sample03.Runtime
{

[BurstCompile]
public partial struct MoveJob : IJobEntity
{
    [ReadOnly] public float Dt;
    [ReadOnly] public ComponentLookup<Parameter> ParamLookUp;
    
    void Execute(
        ref Fish fish,
        ref FishJobData jobData,
        ref LocalTransform lt)
    {
        var param = ParamLookUp[fish.ParamEntity];
        
        var v = fish.Velocity;
        v += fish.Acceleration * Dt;
        var speed = math.length(v);
        speed = math.clamp(speed, param.MinSpeed, param.MaxSpeed);
        var dir = math.normalize(v);
        v = dir * speed;
        fish.Velocity = v;
        
        fish.Acceleration = 0f;
        
        var pos = lt.Position;
        pos += fish.Velocity * Dt;
        lt.Position = pos;
        
        var up = math.up();
        lt.Rotation = quaternion.LookRotationSafe(dir, up);
        
        jobData.Position = lt.Position;
        jobData.Velocity = fish.Velocity;
    }
}

[UpdateAfter(typeof(ForceUpdateSystemGroup))]
public partial struct MoveSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        
        var job = new MoveJob()
        {
            Dt = SystemAPI.Time.DeltaTime,
            ParamLookUp =  _paramLookUp,
        };
        
        var query = SystemAPI
            .QueryBuilder()
            .WithAll<Fish, FishJobData, LocalTransform>()
            .Build();
        state.Dependency = job.ScheduleParallel(query, state.Dependency);
    }
}

}