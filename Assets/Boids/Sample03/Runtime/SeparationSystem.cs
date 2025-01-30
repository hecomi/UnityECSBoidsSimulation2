using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace Boids.Sample03.Runtime
{
    
[BurstCompile]
public partial struct SeparationJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<Parameter> ParamLookUp;
    [ReadOnly] public ComponentLookup<FishJobData> FishJobDataLookUp;
    
    void Execute(
        ref Fish fish,
        in LocalTransform lt,
        in DynamicBuffer<NeighborsEntityBufferElement> neighbors)
    {
        var n = neighbors.Length;
        if (n == 0) return;
        
        var pos = lt.Position;
        
        var forceDir = float3.zero;
        for (int i = 0; i < n; ++i)
        {
            var neighborEntity = neighbors[i].Entity;
            var neighborPos = FishJobDataLookUp[neighborEntity].Position;
            var to = neighborPos - pos;
            var dist = math.length(to);
            var dir = math.normalizesafe(to);
            forceDir += -dir / math.max(dist, 0.1f);
        }
        forceDir /= n;
        forceDir = math.normalizesafe(forceDir);
        
        var param = ParamLookUp[fish.ParamEntity];
        fish.Acceleration += forceDir * param.SeparationForce;
    }
}

[UpdateInGroup(typeof(ForceUpdateSystemGroup))]
public partial struct SeparationSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;
    ComponentLookup<FishJobData> _fishJobDataLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        _fishJobDataLookUp = state.GetComponentLookup<FishJobData>(isReadOnly: true);
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        _fishJobDataLookUp.Update(ref state);
    
        var job = new SeparationJob()
        {
            ParamLookUp = _paramLookUp,
            FishJobDataLookUp = _fishJobDataLookUp,
        };
        
        var query = SystemAPI
            .QueryBuilder()
            .WithAll<Fish, LocalTransform, NeighborsEntityBufferElement>()
            .Build();
        state.Dependency = job.ScheduleParallel(query, state.Dependency);
    }
}

}