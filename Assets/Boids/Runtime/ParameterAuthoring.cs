using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Boids.Runtime
{
    
public struct Parameter : IComponentData
{
    public int Type;
    public float MinSpeed;
    public float MaxSpeed;
    public float3 AreaScale;
    public float AreaDistance;
    public float AreaForce;
    public float NeighborDistance;
    public float NeighborAngle;
    public float SeparationForce;
    public float AlignmentForce;
    public float CohesionForce;
    
    public Parameter(ParameterData param)
    {
        Type = param.Type;
        MinSpeed = param.MinSpeed;
        MaxSpeed = param.MaxSpeed;
        AreaScale = param.AreaScale;
        AreaDistance = param.AreaDistance;
        AreaForce = param.AreaForce;
        NeighborDistance = param.NeighborDistance;
        NeighborAngle = param.NeighborAngle;
        SeparationForce = param.SeparationForce;
        AlignmentForce = param.AlignmentForce;
        CohesionForce = param.CohesionForce;
    }
}

[System.Serializable]
public class ParameterData
{
    [Header("Type")]
    public int Type = 0;
    
    [Header("Move")]
    public float MinSpeed = 2f;
    public float MaxSpeed = 5f;
    
    [Header("Area")]
    public Vector3 AreaScale = Vector3.one * 5f;
    public float AreaDistance = 3f;
    public float AreaForce = 1f;
    
    [Header("Neighbors")]
    public float NeighborDistance = 1f;
    public float NeighborAngle = 90f;
    
    [Header("Separation")]
    public float SeparationForce = 5f;
    
    [Header("Alignment")]
    public float AlignmentForce = 2f;
    
    [Header("Cohesion")]
    public float CohesionForce = 2f;
    
    public void Set(in Parameter param)
    {
        Type = param.Type;
        MinSpeed = param.MinSpeed;
        MaxSpeed = param.MaxSpeed;
        AreaScale = param.AreaScale;
        AreaDistance = param.AreaDistance;
        AreaForce = param.AreaForce;
        NeighborDistance = param.NeighborDistance;
        NeighborAngle = param.NeighborAngle;
        SeparationForce = param.SeparationForce;
        AlignmentForce = param.AlignmentForce;
        CohesionForce = param.CohesionForce;
    }
}

public class ParameterAuthoring : MonoBehaviour
{
    public ParameterData param = new();
}

public class ParameterBaker : Baker<ParameterAuthoring>
{
    public override void Bake(ParameterAuthoring src)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        var param = src.param;
        AddComponent(entity, new Parameter(param));
    }
}

}