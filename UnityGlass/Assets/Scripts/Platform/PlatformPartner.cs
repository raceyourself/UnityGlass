using System;
using UnityEngine;
using System.Threading;

public class PlatformPartner : MonoBehaviour
{

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Platform.Instance.SetMonoBehavioursPartner(this);        
    }

    void Update()
    {
        Platform.Instance.Update();
    }

    public void OnApplicationPause(bool paused)
    {
        Debug.Log("Pause order received to set to "+paused);
    }

    public void OnApplicationFocus(bool paused)
    {
       /* if (Platform.Instance.OnGlass())
        {
            Debug.Log("Focus change order received with " + paused);
            Platform.Instance.OnApplicationFocus(paused);
        }
        else*/
        {
            Debug.Log("Focus change order received with " + paused + " but have not been forwarded");
        }        
    }

    public void OnApplicationQuit()
    {
        Debug.Log("Quit order received");
        Platform.Instance.OnApplicationQuit();
    }

}