using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Boids.Runtime
{
    
public struct Parameter : IComponentData
{
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
}

public class ParameterAuthoring : MonoBehaviour
{
    [Header("Move")]
    public float MinSpeed = 2f;
    public float MaxSpeed = 5f;
    
    [Header("Area")]
    public float3 AreaScale = 5f;
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
}

public class ParameterBaker : Baker<ParameterAuthoring>
{
    public override void Bake(ParameterAuthoring src)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        
        AddComponent(entity, new Parameter()
        {
            MinSpeed = src.MinSpeed,
            MaxSpeed = src.MaxSpeed,
            AreaScale = src.AreaScale,
            AreaDistance = src.AreaDistance,
            AreaForce = src.AreaForce,
            NeighborDistance = src.NeighborDistance,
            NeighborAngle = src.NeighborAngle,
            SeparationForce = src.SeparationForce,
            AlignmentForce = src.AlignmentForce,
            CohesionForce = src.CohesionForce,
        });
    }
}

}