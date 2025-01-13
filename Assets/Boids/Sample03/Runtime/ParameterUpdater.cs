using UnityEngine;

namespace Boids.Sample03.Runtime
{

public class ParameterUpdater : MonoBehaviour
{
    public Parameter param = Parameter.Default;
    bool _isGot = false;
    int _type = 0;

    void OnEnable()
    {
        _type = param.Type;
        _isGot = Parameter.Get(ref param);
    }
    
    void OnDisable()
    {
        _isGot = false;
    }

    void Update()
    {
        if (param.Type != _type)
        {
            _isGot = false;
        }
        
        if (!_isGot)
        {
            _isGot = Parameter.Get(ref param);
            if (_isGot) 
            {
                _type = param.Type;
            }
        }
        else
        {
            Parameter.Set(param);
        }
    }
}

}