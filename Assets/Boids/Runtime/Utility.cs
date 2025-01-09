using Unity.Entities;
using Unity.Collections;

namespace Boids.Runtime
{

public static class Utility
{
    public static bool SetParameter(ParameterData paramData)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(ComponentType.ReadWrite<Parameter>());
        var entities = query.ToEntityArray(Allocator.Temp);
        if (entities.Length == 0) return false;
        
        bool set = false;
        foreach (var entity in entities)
        {
            var parameter = manager.GetComponentData<Parameter>(entity);
            if (parameter.Type != paramData.Type) continue;
            parameter = new Parameter(paramData);
            manager.SetComponentData(entity, parameter);
            set = true;
        }
        return set;
    }
    
    public static bool GetParameter(ParameterData paramData)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = manager.CreateEntityQuery(ComponentType.ReadOnly<Parameter>());
        var parameters = query.ToComponentDataArray<Parameter>(Allocator.Temp);
        if (parameters.Length == 0) return false;
        
        foreach (var parameter in parameters)
        {
            if (parameter.Type != paramData.Type) continue;
            paramData.Set(parameter);
            return true;
        }
        return false;
    }
}

}