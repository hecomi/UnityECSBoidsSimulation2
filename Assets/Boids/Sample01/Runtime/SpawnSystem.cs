using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Boids.Sample01.Runtime
{

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (school, param, lt, ptm, entity) in 
            SystemAPI.Query<
                RefRW<School>,
                RefRO<Parameter>,
                RefRO<LocalTransform>,
                RefRO<PostTransformMatrix>>().WithEntityAccess())
        {
            if (school.ValueRO.Initialized) continue;
            
            var transform = math.mul(lt.ValueRO.ToMatrix(), ptm.ValueRO.Value);
            Create(ref state, school.ValueRO, param.ValueRO, transform, entity);
            school.ValueRW.Initialized = true;
        }
    }
    
    void Create(
        ref SystemState state, 
        in School school, 
        in Parameter param, 
        in float4x4 areaTransform,
        Entity groupEntity)
    {
        var prefab = school.Prefab;
        if (prefab == Entity.Null) return;
            
        var manager = state.EntityManager;
        var entities = manager.Instantiate(school.Prefab, school.SpawnCount, Allocator.Temp);
        var random = new Random(school.RandomSeed);
        
        foreach (var entity in entities)
        {
            var lt = SystemAPI.GetComponentRW<LocalTransform>(entity);
            
            var localPos = random.NextFloat3() - 0.5f;
            lt.ValueRW.Position = math.transform(areaTransform, localPos);
            
            var dir = random.NextFloat3Direction();
            var up = math.up();
            lt.ValueRW.Rotation = quaternion.LookRotation(dir, up);
            
            var fish = SystemAPI.GetComponentRW<Fish>(entity);
            fish.ValueRW.Velocity = dir * param.MinSpeed;
            fish.ValueRW.Acceleration = 0f;
            fish.ValueRW.ParamEntity = groupEntity;
        }
    }
}

}
