using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Sample01.Runtime
{
    
public partial struct NeighborsDetectionSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;
    BufferLookup<NeighborsEntityBuffer> _neighborsLookUp;
    EntityQuery _query;

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        _neighborsLookUp = state.GetBufferLookup<NeighborsEntityBuffer>(isReadOnly: false);
        _query = SystemAPI.QueryBuilder().WithAll<Fish, LocalTransform>().Build();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        _neighborsLookUp.Update(ref state);
        
        using var entities = _query.ToEntityArray(Allocator.Temp);
        using var localTransforms = _query.ToComponentDataArray<LocalTransform>(Allocator.Temp); 
        using var fishes = _query.ToComponentDataArray<Fish>(Allocator.Temp);
        
        for (int i = 0; i < entities.Length; ++i)
        {
            var entity0 = entities[i];
            var neighbors0 = _neighborsLookUp[entity0];
            neighbors0.Clear();
            
            var fish0 = fishes[i];
            var param = _paramLookUp[fish0.ParamEntity];
            
            var neighborAngle = math.radians(param.NeighborAngle);
            var neighborDist = param.NeighborDistance;
            var prodThresh = math.cos(neighborAngle);
            
            var lt0 = localTransforms[i];
            var pos0 = lt0.Position;
            var fwd0 = math.normalizesafe(fish0.Velocity);
            
            for (int j = 0; j < entities.Length; ++j)
            {
                if (i == j) continue;
                
                var lt1 = localTransforms[j];
                var pos1 = lt1.Position;
                var to = pos1 - pos0;
                var dist = math.length(to);
                if (dist > neighborDist) continue;
                
                var dir = to / math.max(dist, 1e-3f);
                var prod = math.dot(dir, fwd0);
                if (prod < prodThresh) continue;
                
                var entity1 = entities[j];
                var elem = new NeighborsEntityBuffer() { Entity = entity1 };
                neighbors0.Add(elem);
                
                if (neighbors0.Length == neighbors0.Capacity) break;
            }
        }
    }
}

}