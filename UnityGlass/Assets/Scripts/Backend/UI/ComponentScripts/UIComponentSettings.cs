using UnityEngine;
using System.Collections;

/// <summary>
/// generic component class for data forwarding into deeper parts of the prefab construction. 
/// </summary>
public class UIComponentSettings : MonoBehaviour, IDataVaultListener
{

    /// <summary>
    /// apply is called as soon new value of the registered listener is recorded
    /// </summary>
    /// <returns></returns>
    public virtual void Apply(string identifier)
    {

    }

    /// <summary>
    /// is called when data listener is about to get registered
    /// </summary>
    /// <returns></returns>
    public virtual void Register()
    {

    }

    /// <summary>
    /// unregisters component from database events
    /// </summary>
    /// <returns></returns>
    public virtual void OnDestroy()
    {
        DataVault.UnRegisterListner(this);
    }
}

