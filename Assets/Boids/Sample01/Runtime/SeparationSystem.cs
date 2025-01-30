using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Sample01.Runtime
{

[UpdateBefore(typeof(MoveSystem))]
[UpdateAfter(typeof(NeighborsDetectionSystem))]
public partial struct SeparationSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;
    ComponentLookup<LocalTransform> _transformLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        _transformLookUp = state.GetComponentLookup<LocalTransform>(isReadOnly: true);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        _transformLookUp.Update(ref state);
    
        foreach (var (fish, lt, neighbors) in 
            SystemAPI.Query<
                RefRW<Fish>,
                RefRO<LocalTransform>,
                DynamicBuffer<NeighborsEntityBufferElement>>())
        {
            var n = neighbors.Length;
            if (n == 0) continue;
            
            var pos = lt.ValueRO.Position;
            
            var forceDir = float3.zero;
            for (int i = 0; i < n; ++i)
            {
                var neighborEntity = neighbors[i].Entity;
                var neighborPos = _transformLookUp[neighborEntity].Position;
                var to = neighborPos - pos;
                var dist = math.length(to);
                var dir = math.normalizesafe(to);
                forceDir += -dir / math.max(dist, 0.1f);
            }
            forceDir /= n;
            forceDir = math.normalizesafe(forceDir);
            
            var param = _paramLookUp[fish.ValueRW.ParamEntity];
            fish.ValueRW.Acceleration += forceDir * param.SeparationForce;
        }
    }
}

}