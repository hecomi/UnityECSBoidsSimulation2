using UnityEngine;

namespace Boids.Runtime
{

public class ParameterUpdater : MonoBehaviour
{
    public ParameterData paramData;
    bool _isGot = false;
    int _type = 0;

    void OnEnable()
    {
        _type = paramData.Type;
        _isGot = Utility.GetParameter(paramData);
    }
    
    void OnDisable()
    {
        _isGot = false;
    }

    void Update()
    {
        if (paramData.Type != _type)
        {
            _isGot = false;
        }
        
        if (!_isGot)
        {
            _isGot = Utility.GetParameter(paramData);
            if (_isGot) 
            {
                _type = paramData.Type;
            }
        }
        else
        {
            Utility.SetParameter(paramData);
        }
    }
}

}