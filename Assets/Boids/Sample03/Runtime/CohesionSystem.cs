using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Sample03.Runtime
{
    
[BurstCompile]
public partial struct CohesionJob : IJobEntity
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

        var averagePos = float3.zero;
        for (int i = 0; i < n; ++i)
        {
            var neighborEntity = neighbors[i].Entity;
            var neighborPos = FishJobDataLookUp[neighborEntity].Position;
            averagePos += neighborPos;
        }
        averagePos /= n;
        
        var pos = lt.Position;
        var param = ParamLookUp[fish.ParamEntity];
        fish.Acceleration += (averagePos - pos) * param.CohesionForce;
    }
}

[UpdateInGroup(typeof(ForceUpdateSystemGroup))]
public partial struct CohesionSystem : ISystem
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
    
        var job = new CohesionJob()
        {
            FishJobDataLookUp = _fishJobDataLookUp,
            ParamLookUp =  _paramLookUp,
        };
        
        var query = SystemAPI
            .QueryBuilder()
            .WithAll<Fish, LocalTransform, NeighborsEntityBufferElement>()
            .Build();
        state.Dependency = job.ScheduleParallel(query, state.Dependency);
    }
}

}