using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids.Runtime
{

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (school, param, entity) in 
            SystemAPI.Query<
                RefRW<School>,
                RefRO<Parameter>>().WithEntityAccess())
        {
            if (school.ValueRO.Initialized) continue;
            Create(ref state, school.ValueRO, param.ValueRO, entity);
            school.ValueRW.Initialized = true;
        }
    }
    
    void Create(
        ref SystemState state, 
        in School school, 
        in Parameter param, 
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
            
            var pos = (random.NextFloat3() - 1f) / 2f;
            pos *= param.AreaScale * 0.5f;
            lt.ValueRW.Position = pos;
            
            var dir = random.NextFloat3Direction();
            var up = math.up();
            lt.ValueRW.Rotation = quaternion.LookRotation(dir, up);
            
            var fish = SystemAPI.GetComponentRW<Fish>(entity);
            fish.ValueRW.Velocity = dir;
            fish.ValueRW.Acceleration = 0f;
            fish.ValueRW.ParamEntity = groupEntity;
        }
    }
}

}
