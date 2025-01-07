using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Runtime
{

public partial struct AreaSystem : ISystem
{
    ComponentLookup<Parameter> _paramLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _paramLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _paramLookUp.Update(ref state);
        
        var dt = SystemAPI.Time.DeltaTime;
    
        foreach (var (fish, lt) in 
            SystemAPI.Query<
                RefRW<Fish>,
                RefRO<LocalTransform>>())
        {
            var param = _paramLookUp.GetRefRO(fish.ValueRO.ParamEntity);
            var scale = param.ValueRO.AreaScale * 0.5f;
            var thresh = param.ValueRO.AreaDistance;
            var weight = param.ValueRO.AreaForce;
            
            float3 pos = lt.ValueRO.Position;
            fish.ValueRW.Acceleration +=
                GetAccelAgainstWall(pos.x - -scale.x, math.right(), thresh, weight) +
                GetAccelAgainstWall(pos.y - -scale.y, math.up(), thresh, weight) +
                GetAccelAgainstWall(pos.z - -scale.z, math.forward(), thresh, weight) +
                GetAccelAgainstWall(+scale.x - pos.x, math.left(), thresh, weight) +
                GetAccelAgainstWall(+scale.y - pos.y, math.down(), thresh, weight) +
                GetAccelAgainstWall(+scale.z - pos.z, math.back(), thresh, weight);
        }
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

}