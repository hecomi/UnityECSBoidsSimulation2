using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Sample03.Runtime
{
    
[BurstCompile]
internal static class NeighborsDetectionUtil
{
    [BurstCompile]
    public static int GetHash(in int3 cell)
    {
        return cell.x * 73856093 ^ cell.y * 19349663 ^ cell.z * 83492791;
    }
}

[BurstCompile]
public struct NeighborsHashMapBuildJob : IJobParallelFor
{
    [WriteOnly] public NativeParallelMultiHashMap<int, int>.ParallelWriter HashMap;
    [ReadOnly] public float CellSize;
    [ReadOnly] public NativeArray<LocalTransform> LocalTransforms;
    
    public void Execute(int index)
    {
        var pos = LocalTransforms[index].Position;
        var cell = (int3)(pos / CellSize);
        var hash = NeighborsDetectionUtil.GetHash(cell);
        HashMap.Add(hash, index);
    }
}

[BurstCompile]
public struct NeighborsDetectionJob : IJobParallelFor
{
    [WriteOnly] public NativeParallelMultiHashMap<Entity, NeighborsEntityBufferElement>.ParallelWriter Neighbors;
    [ReadOnly] public NativeArray<Entity> Entities;
    [ReadOnly] public NativeArray<Fish> Fishes;
    [ReadOnly] public NativeArray<LocalTransform> LocalTransforms;
    [ReadOnly] public ComponentLookup<Parameter> ParamLookUp;
    [ReadOnly] public NativeParallelMultiHashMap<int, int> HashMap;
    [ReadOnly] public float CellSize;
    [ReadOnly] public NativeArray<int3> CellOffsets;
    
    public void Execute(int index)
    {
        var posSelf = LocalTransforms[index].Position;
        var cellSelf = (int3)(posSelf / CellSize);
        
        var fishSelf = Fishes[index];
        var forwardSelf = math.normalizesafe(fishSelf.Velocity);
        
        var param = ParamLookUp[fishSelf.ParamEntity];
        var neighborAngle = math.radians(param.NeighborAngle);
        var neighborDist = param.NeighborDistance;
        var prodThresh = math.cos(neighborAngle);
        
        var entitySelf = Entities[index];
        int neighborsCount = 0;
        bool isNeighborCountFull = false;
        int maxNeighborsCount = FishConfig.NeighborsEntityBufferMaxSize;
        
        for (int offsetIndex = 0; offsetIndex < CellOffsets.Length; ++offsetIndex)
        {
            var cell = cellSelf + CellOffsets[offsetIndex];
            var hashSelf = NeighborsDetectionUtil.GetHash(cell);
            if (!HashMap.TryGetFirstValue(hashSelf, out var j, out var it)) continue;
            
            do
            {
                var entityTarget = Entities[j];
                if (entitySelf == entityTarget) continue;
                
                var ltTarget = LocalTransforms[j];
                var posTarget = ltTarget.Position;
                var to = posTarget - posSelf;
                var dist = math.length(to);
                if (dist > neighborDist) continue;
        
                var dir = to / math.max(dist, 1e-3f);
                var prod = math.dot(dir, forwardSelf);
                if (prod < prodThresh) continue;
        
                var elem = new NeighborsEntityBufferElement() { Entity = entityTarget };
                Neighbors.Add(entitySelf, elem);
                
                ++neighborsCount;
                isNeighborCountFull = neighborsCount >= maxNeighborsCount;
                if (isNeighborCountFull) break;
            } while (HashMap.TryGetNextValue(out j, ref it));
            
            if (isNeighborCountFull) break;
        }
    }
}

[BurstCompile]
public partial struct NeighborsWriteJob : IJobEntity
{
    [ReadOnly] public NativeParallelMultiHashMap<Entity, NeighborsEntityBufferElement> Neighbors;
    
    public void Execute(
        Entity entity,
        ref DynamicBuffer<NeighborsEntityBufferElement> buffer)
    {
        buffer.Clear();
        if (!Neighbors.TryGetFirstValue(entity, out var elem, out var it)) return;
        do
        {
            buffer.Add(elem);
        } while (Neighbors.TryGetNextValue(out elem, ref it));
    }
}

[BurstCompile]
public struct NeighborsCleanUpJob : IJob
{
    [DeallocateOnJobCompletion][ReadOnly] public NativeArray<Entity> Entities;
    [DeallocateOnJobCompletion][ReadOnly] public NativeArray<Fish> Fishes;
    [DeallocateOnJobCompletion][ReadOnly] public NativeArray<LocalTransform> LocalTransforms;
    public void Execute() {}
}

[UpdateBefore(typeof(ForceUpdateSystemGroup))]
public partial struct NeighborsDetectionSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;
    BufferLookup<NeighborsEntityBufferElement> _neighborsLookUp;
    NativeParallelMultiHashMap<int, int> _hashMap;
    NativeParallelMultiHashMap<Entity, NeighborsEntityBufferElement> _neighborMap;
    EntityQuery _fishQuery;
    EntityQuery _paramQuery;
    NativeArray<int3> _cellOffsets;

    [BurstCompile]
    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        _neighborsLookUp = state.GetBufferLookup<NeighborsEntityBufferElement>(isReadOnly: false);
        _hashMap = new NativeParallelMultiHashMap<int, int>(100, Allocator.Persistent);
        _neighborMap = new NativeParallelMultiHashMap<Entity, NeighborsEntityBufferElement>(100, Allocator.Persistent);
        _fishQuery = SystemAPI.QueryBuilder().WithAll<Fish, LocalTransform, NeighborsEntityBufferElement>().Build();
        _paramQuery = SystemAPI.QueryBuilder().WithAll<Parameter>().Build();
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
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state) 
    {
        if (_cellOffsets.IsCreated) _cellOffsets.Dispose();
        if (_hashMap.IsCreated) _hashMap.Dispose();
        if (_neighborMap.IsCreated) _neighborMap.Dispose();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        _neighborsLookUp.Update(ref state);
        
        var entities = _fishQuery.ToEntityArray(Allocator.TempJob);
        var fishes = _fishQuery.ToComponentDataArray<Fish>(Allocator.TempJob);
        var localTransforms = _fishQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        
        using var parameters = _paramQuery.ToComponentDataArray<Parameter>(Allocator.Temp);
        float cellSize = 0.1f;
        for (int i = 0; i < parameters.Length; ++i)
        {
            var param = parameters[i];
            cellSize = math.max(cellSize, param.NeighborDistance * 0.5f);
        }
        
        _hashMap.Clear();
        int n = fishes.Length;
        if (_hashMap.Capacity < n) _hashMap.Capacity = n;
        var hashMapBuildJob = new NeighborsHashMapBuildJob()
        {
            HashMap = _hashMap.AsParallelWriter(),
            CellSize = cellSize,
            LocalTransforms = localTransforms,
        };
        state.Dependency = hashMapBuildJob.Schedule(n, 32, state.Dependency);
        
        _neighborMap.Clear();
        var maxBufferSize = n * FishConfig.NeighborsEntityBufferMaxSize;
        if (_neighborMap.Capacity < maxBufferSize) _neighborMap.Capacity = maxBufferSize;
        var detectionJob = new NeighborsDetectionJob()
        {
            Entities = entities,
            Fishes = fishes,
            LocalTransforms = localTransforms,
            ParamLookUp = _paramLookUp,
            HashMap = _hashMap,
            Neighbors = _neighborMap.AsParallelWriter(),
            CellSize = cellSize,
            CellOffsets = _cellOffsets,
        };
        state.Dependency = detectionJob.Schedule(n, 8, state.Dependency);
        
        var writeJob = new NeighborsWriteJob()
        {
            Neighbors = _neighborMap,
        };
        state.Dependency = writeJob.ScheduleParallel(_fishQuery, state.Dependency);
        
        var cleanUpJob = new NeighborsCleanUpJob()
        {
            Entities = entities,
            Fishes = fishes,
            LocalTransforms = localTransforms,
        };
        state.Dependency = cleanUpJob.Schedule(state.Dependency);
    }
}

}