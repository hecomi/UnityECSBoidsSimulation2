using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Sample03.Runtime
{
    
[BurstCompile]
public partial struct AreaJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<Parameter> ParamLookUp;
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookUp;
    [ReadOnly] public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookUp;
    
    void Execute(
        ref Fish fish,
        in FishJobData jobData)
    {
        var paramEntity = fish.ParamEntity;
        var param = ParamLookUp[paramEntity];
        var areaLt = TransformLookUp[paramEntity];
        var areaPtm = PostTransformMatrixLookUp[paramEntity];
        var scale = areaPtm.Value.Scale() * 0.5f;
        var thresh = param.AreaDistance;
        var weight = param.AreaForce;
        
        var pos = jobData.Position;
        var transformRt = float4x4.TRS(areaLt.Position, areaLt.Rotation, 1f);
        pos = math.transform(math.inverse(transformRt), pos);
        var addAccel =
            GetAccelAgainstWall(pos.x - -scale.x, math.right(), thresh, weight) +
            GetAccelAgainstWall(pos.y - -scale.y, math.up(), thresh, weight) +
            GetAccelAgainstWall(pos.z - -scale.z, math.forward(), thresh, weight) +
            GetAccelAgainstWall(+scale.x - pos.x, math.left(), thresh, weight) +
            GetAccelAgainstWall(+scale.y - pos.y, math.down(), thresh, weight) +
            GetAccelAgainstWall(+scale.z - pos.z, math.back(), thresh, weight);
        addAccel = math.rotate(areaLt.Rotation, addAccel);
        fish.Acceleration += addAccel;
    }
    
    float3 GetAccelAgainstWall(float dist, float3 dir, float thresh, float weight)
    {
        if (dist < thresh)
        {
            dist = math.max(dist, 0.01f);
            var a = dist / math.max(thresh, 0.01f);
            return dir * (weight / a);
        }
        return float3.zero;
    }
}
    
[UpdateInGroup(typeof(ForceUpdateSystemGroup))]
public partial struct AreaSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;
    ComponentLookup<LocalTransform> _transformLookUp;
    ComponentLookup<PostTransformMatrix> _postTransformMatrixLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
        _transformLookUp = state.GetComponentLookup<LocalTransform>(isReadOnly: true);
        _postTransformMatrixLookUp = state.GetComponentLookup<PostTransformMatrix>(isReadOnly: true);
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        _transformLookUp.Update(ref state);
        _postTransformMatrixLookUp.Update(ref state);
        
        var job = new AreaJob()
        {
            ParamLookUp = _paramLookUp,
            TransformLookUp = _transformLookUp,
            PostTransformMatrixLookUp = _postTransformMatrixLookUp,
        };
        
        var query = SystemAPI
            .QueryBuilder()
            .WithAll<Fish, FishJobData>()
            .Build();
        state.Dependency = job.ScheduleParallel(query, state.Dependency);
    }
    
}

}