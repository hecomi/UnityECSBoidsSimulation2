using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Boids.Runtime
{
    
public class DebugViewer : MonoBehaviour
{
    public bool area = false;
    
    void OnDrawGizmos()
    {
        if (area) DrawAreas();
    }
    
    void DrawAreas()
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(
            ComponentType.ReadOnly<Parameter>(),
            ComponentType.ReadOnly<LocalTransform>());
        var entities = query.ToEntityArray(Allocator.Temp);
        
        foreach (var entity in entities)
        {
            var param = manager.GetComponentData<Parameter>(entity);
            var lt = manager.GetComponentData<LocalTransform>(entity);
            var colorFloat3 = param.DebugDrawAreaColor * 255f;
           
            Gizmos.color = new Color(colorFloat3.x, colorFloat3.y, colorFloat3.z, 1f);
            Gizmos.matrix = Matrix4x4.TRS(lt.Position, lt.Rotation, param.AreaScale);
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
#if UNITY_EDITOR
            Handles.Label(lt.Position, "Boids");
#endif
        }
    }
}

}