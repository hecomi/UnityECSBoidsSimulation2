using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Boids.Sample02.Runtime
{
    
public class DebugViewer : MonoBehaviour
{
    [Header("Area")]
    public bool drawAreas = false;
    
    [Header("Boid")]
    public bool drawBoids = false;
    public Vector3 boidScale = Vector3.one;
    
    void OnDrawGizmos()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;
        
        if (drawAreas) DrawAreas();
        if (drawBoids) DrawBoids();
    }
    
    void DrawAreas()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(
            ComponentType.ReadOnly<Parameter>(),
            ComponentType.ReadOnly<LocalTransform>(),
            ComponentType.ReadOnly<PostTransformMatrix>());
        var entities = query.ToEntityArray(Allocator.Temp);
        
        foreach (var entity in entities)
        {
            var param = manager.GetComponentData<Parameter>(entity);
            var lt = manager.GetComponentData<LocalTransform>(entity);
            var ptm = manager.GetComponentData<PostTransformMatrix>(entity);
            Gizmos.color = (Vector4)(new float4(param.DebugAreaColor, 1f));
            Gizmos.matrix = math.mul(lt.ToMatrix(), ptm.Value);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
#if UNITY_EDITOR
            Handles.Label(lt.Position, "Boids");
#endif
        }
    }
    
    void DrawBoids()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(
            ComponentType.ReadOnly<Fish>(),
            ComponentType.ReadOnly<LocalTransform>());
        var entities = query.ToEntityArray(Allocator.Temp);
        
        foreach (var entity in entities)
        {
            var lt = manager.GetComponentData<LocalTransform>(entity);
            Gizmos.color = Color.gray;
            Gizmos.matrix = lt.ToMatrix();
            Gizmos.DrawWireCube(Vector3.zero, boidScale);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}

}