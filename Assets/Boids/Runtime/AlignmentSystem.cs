using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Boids.Runtime
{

public partial struct AlignmentSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;
    ComponentLookup<Fish> _fishLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        _fishLookUp = state.GetComponentLookup<Fish>(isReadOnly: true);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        _fishLookUp.Update(ref state);
    
        foreach (var (fish, neighbors) in 
            SystemAPI.Query<
                RefRW<Fish>,
                DynamicBuffer<NeighborsEntityBuffer>>())
        {
            var n = neighbors.Length;
            if (n == 0) continue;

            var averageV = float3.zero;
            for (int i = 0; i < n; ++i)
            {
                var neighborEntity = neighbors[i].Entity;
                var neighborV = _fishLookUp[neighborEntity].Velocity;
                averageV += neighborV;
            }
            averageV /= n;
            
            var param = _paramLookUp[fish.ValueRW.ParamEntity];
            var v = fish.ValueRO.Velocity;
            
            fish.ValueRW.Acceleration += (averageV - v) * param.AlignmentForce;
        }
    }
}

}