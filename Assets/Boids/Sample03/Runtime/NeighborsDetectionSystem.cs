using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Sample03.Runtime
{

[BurstCompile]
public partial struct NeighborsDetectionSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;
    BufferLookup<NeighborsEntityBuffer> _neighborsLookUp;
    EntityQuery _query;
    float _cellsize;
    NativeArray<int3> _cellOffsets;
    
    int GetHash(int3 cell)
    {
        return cell.x * 73856093 ^ cell.y * 19349663 ^ cell.z * 83492791;
    }

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        _neighborsLookUp = state.GetBufferLookup<NeighborsEntityBuffer>(isReadOnly: false);
        _query = SystemAPI.QueryBuilder().WithAll<Fish, LocalTransform>().Build();
        _cellsize = 0.5f;
        _cellOffsets = new NativeArray<int3>(27, Allocator.Persistent);
        {
            var i = 0;
            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    for (int z = -1; z <= 1; ++z)
                    {
                        _cellOffsets[i++] = new int3(x, y, z);
                    }
                }
            }
        }
    }
    
    public void OnDestroy(ref SystemState state) 
    {
        if (_cellOffsets.IsCreated) _cellOffsets.Dispose();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        _neighborsLookUp.Update(ref state);
        
        using var entities = _query.ToEntityArray(Allocator.Temp);
        using var fishes = _query.ToComponentDataArray<Fish>(Allocator.Temp);
        using var localTransforms = _query.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        
        int n = fishes.Length;
        using var hashMap = new NativeParallelMultiHashMap<int, int>(n, Allocator.Temp);
        float nextCellSize = 0.1f;
        
        for (int i = 0; i < n; ++i)
        {
            var pos = localTransforms[i].Position;
            var cell = (int3)(pos / _cellsize);
            var hash = GetHash(cell);
            hashMap.Add(hash, i);
            
            var fish = fishes[i];
            var paramEntity = fish.ParamEntity;
            var param = _paramLookUp[paramEntity];
            nextCellSize = math.max(nextCellSize, param.NeighborDistance * 0.5f);
        }
        
        for (int i = 0; i < n; ++i)
        {
            var pos0 = localTransforms[i].Position;
            var cell0 = (int3)(pos0 / _cellsize);
            
            var fish0 = fishes[i];
            var fwd0 = math.normalizesafe(fish0.Velocity);
            
            var param = _paramLookUp[fish0.ParamEntity];
            var neighborAngle = math.radians(param.NeighborAngle);
            var neighborDist = param.NeighborDistance;
            var prodThresh = math.cos(neighborAngle);
            
            var entity0 = entities[i];
            var neighbors0 = _neighborsLookUp[entity0];
            neighbors0.Clear();
            
            for (int offsetIndex = 0; offsetIndex < _cellOffsets.Length; ++offsetIndex)
            {
                var hash0 = GetHash(cell0 + _cellOffsets[offsetIndex]);
                if (!hashMap.TryGetFirstValue(hash0, out var j, out var it)) continue;
                
                do
                {
                    var entity1 = entities[j];
                    if (entity0 == entity1) continue;
                    
                    var lt1 = localTransforms[j];
                    var pos1 = lt1.Position;
                    var to = pos1 - pos0;
                    var dist = math.length(to);
                    if (dist > neighborDist) continue;
            
                    var dir = to / math.max(dist, 1e-3f);
                    var prod = math.dot(dir, fwd0);
                    if (prod < prodThresh) continue;
            
                    var elem = new NeighborsEntityBuffer() { Entity = entity1 };
                    neighbors0.Add(elem);
                    if (neighbors0.Length >= neighbors0.Capacity) break;
                } while (hashMap.TryGetNextValue(out j, ref it));
            }
        }
        
        _cellsize = nextCellSize;
    }
}

}