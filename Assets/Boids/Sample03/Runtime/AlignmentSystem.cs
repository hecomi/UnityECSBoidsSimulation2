using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Boids.Sample03.Runtime
{
    
[BurstCompile]
public partial struct AlignmentJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<Parameter> ParamLookUp;
    [ReadOnly] public ComponentLookup<FishJobData> FishJobDataLookUp;
    
    void Execute(
        ref Fish fish,
        in DynamicBuffer<NeighborsEntityBufferElement> neighbors)
    {
        var n = neighbors.Length;
        if (n == 0) return;

        var averageV = float3.zero;
        for (int i = 0; i < n; ++i)
        {
            var neighborEntity = neighbors[i].Entity;
            var neighborV = FishJobDataLookUp[neighborEntity].Velocity;
            averageV += neighborV;
        }
        averageV /= n;
        
        var param = ParamLookUp[fish.ParamEntity];
        var v = fish.Velocity;
        
        fish.Acceleration += (averageV - v) * param.AlignmentForce;
    }
}

[UpdateInGroup(typeof(ForceUpdateSystemGroup))]
public partial struct AlignmentSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;
    ComponentLookup<FishJobData> _fishJobDataLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        _fishJobDataLookUp = state.GetComponentLookup<FishJobData>(isReadOnly: true);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        _fishJobDataLookUp.Update(ref state);
    
        var job = new AlignmentJob()
        {
            FishJobDataLookUp = _fishJobDataLookUp,
            ParamLookUp =  _paramLookUp,
        };
        
        var query = SystemAPI
            .QueryBuilder()
            .WithAll<Fish, NeighborsEntityBufferElement>()
            .Build();
        state.Dependency = job.ScheduleParallel(query, state.Dependency);
    }
}

}