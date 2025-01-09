using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Boids.Runtime
{

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
                DynamicBuffer<NeighborsEntityBuffer>>())
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
                forceDir += -math.normalizesafe(to);
            }
            forceDir /= n;
            
            var param = _paramLookUp[fish.ValueRW.ParamEntity];
            fish.ValueRW.Acceleration += forceDir * param.SeparationForce;
        }
    }
}

}