using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{

public partial struct WallSystem : ISystem
{
    ComponentLookup<Parameter> _ParamLookUp;

    public void OnCreate(ref SystemState state) 
    {
        _ParamLookUp = state.GetComponentLookup<Parameter>(isReadOnly: true);
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _ParamLookUp.Update(ref state);
        
        var dt = SystemAPI.Time.DeltaTime;
    
        foreach (var (fish, lt) in 
            SystemAPI.Query<
                RefRW<Fish>,
                RefRO<LocalTransform>>())
        {
            var param = _ParamLookUp.GetRefRO(fish.ValueRO.ParamEntity);
            var scale = param.ValueRO.AreaScale * 0.5f;
            var thresh = param.ValueRO.WallDistance;
            var weight = param.ValueRO.WallForce;
            
            float3 pos = lt.ValueRO.Position;
            fish.ValueRW.Acceleration +=
                GetAccelAgainstWall(-scale.x - pos.x, math.right(), thresh, weight) +
                GetAccelAgainstWall(-scale.y - pos.y, math.up(), thresh, weight) +
                GetAccelAgainstWall(-scale.z - pos.z, math.forward(), thresh, weight) +
                GetAccelAgainstWall(+scale.x - pos.x, math.left(), thresh, weight) +
                GetAccelAgainstWall(+scale.y - pos.y, math.down(), thresh, weight) +
                GetAccelAgainstWall(+scale.z - pos.z, math.back(), thresh, weight);
        }
    }
    
    float3 GetAccelAgainstWall(float dist, float3 dir, float thresh, float weight)
    {
        if (dist < thresh)
        {
            return dir * (weight / math.abs(dist / thresh));
        }
        return float3.zero;
    }
}

}