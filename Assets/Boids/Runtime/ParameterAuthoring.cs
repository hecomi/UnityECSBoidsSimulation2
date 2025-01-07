using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace Boids
{
    
public struct Parameter : IComponentData
{
    public float MinSpeed;
    public float MaxSpeed;
    public float3 AreaScale;
    public float WallDistance;
    public float WallForce;
    public float AlignmentForce;
    public float CohesionForce;
}

public class ParameterAuthoring : MonoBehaviour
{
    [Header("Move")]
    public float MinSpeed = 2f;
    public float MaxSpeed = 5f;
    
    [Header("Wall")]
    public float3 WallScale = 5f;
    public float WallDistance = 3f;
    public float WallForce = 1f;
    
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
            AreaScale = src.WallScale,
            WallDistance = src.WallDistance,
            WallForce = src.WallForce,
            AlignmentForce = src.AlignmentForce,
            CohesionForce = src.CohesionForce,
        });
    }
}

}