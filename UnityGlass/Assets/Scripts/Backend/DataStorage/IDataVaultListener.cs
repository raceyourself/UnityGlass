using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public interface IDataVaultListener 
{
    /// <summary>
    /// This function is called every time we want to update component registered attributes
    /// </summary>
    /// <returns></returns>
    void Apply(string identifier);

    /// <summary>
    /// This function is called once at the beginning of script live to ensure everything is ready and can respond to registrar call. 
    /// For example this is the moment component might register for database events
    /// </summary>
    /// <returns></returns>
    void Register();

    /// <summary>
    /// ensure that your instance unregisters itself after usage
    /// </summary>
    /// <returns></returns>
    void OnDestroy();
}
