using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Sample02.Runtime
{

[UpdateBefore(typeof(MoveSystem))]
[UpdateAfter(typeof(NeighborsDetectionSystem))]
public partial struct CohesionSystem : ISystem
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

            var averagePos = float3.zero;
            for (int i = 0; i < n; ++i)
            {
                var neighborEntity = neighbors[i].Entity;
                var neighborPos = _transformLookUp[neighborEntity].Position;
                averagePos += neighborPos;
            }
            averagePos /= n;
            
            var pos = lt.ValueRO.Position;
            var param = _paramLookUp[fish.ValueRW.ParamEntity];
            fish.ValueRW.Acceleration += (averagePos - pos) * param.CohesionForce;
        }
    }
}

}