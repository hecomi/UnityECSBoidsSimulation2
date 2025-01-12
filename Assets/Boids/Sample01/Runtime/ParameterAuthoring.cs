using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

namespace Boids.Sample01.Runtime
{
    
[System.Serializable]
public struct Parameter : IComponentData
{
    public int Type;
    
    [Header("Move")]
    public float MinSpeed;
    public float MaxSpeed;
    
    [Header("Area")]
    public float AreaDistance;
    public float AreaForce;
    
    [Header("Neighbors")]
    public float NeighborDistance;
    public float NeighborAngle;
    
    [Header("Separation")]
    public float SeparationForce;
    
    [Header("Alignment")]
    public float AlignmentForce;
    
    [Header("Cohesion")]
    public float CohesionForce;
    
#if UNITY_EDITOR
    [Header("Debug")]
    public float3 DebugAreaColor;
#endif
    
    public static Parameter Default
    {
        get => new Parameter()
        {
            Type = 0,
            MinSpeed = 2f,
            MaxSpeed = 5f,
            AreaDistance = 3f,
            AreaForce = 1f,
            NeighborDistance = 1f,
            NeighborAngle = 90f,
            SeparationForce = 5f,
            AlignmentForce = 5f,
            CohesionForce = 5f,
            DebugAreaColor = 1f,
        };
    }
    
    public static bool Set(in Parameter newParam)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(ComponentType.ReadWrite<Parameter>());
        var entities = query.ToEntityArray(Allocator.Temp);
        if (entities.Length == 0) return false;
        
        bool set = false;
        foreach (var entity in entities)
        {
            var param = manager.GetComponentData<Parameter>(entity);
            if (param.Type != newParam.Type) continue;
            manager.SetComponentData(entity, newParam);
            set = true;
        }
        return set;
    }
    
    public static bool Get(ref Parameter outParam)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(ComponentType.ReadOnly<Parameter>());
        var parameters = query.ToComponentDataArray<Parameter>(Allocator.Temp);
        if (parameters.Length == 0) return false;
        
        foreach (var param in parameters)
        {
            if (param.Type != outParam.Type) continue;
            outParam = param;
            return true;
        }
        return false;
    }
}

public class ParameterAuthoring : MonoBehaviour
{
    public Parameter param = Parameter.Default;
}

public class ParameterBaker : Baker<ParameterAuthoring>
{
    public override void Bake(ParameterAuthoring src)
    {
        var entity = GetEntity(TransformUsageFlags.NonUniformScale);
        AddComponent(entity, src.param);
    }
}

}