using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System;
using System.Runtime.CompilerServices;
using System.Collections;
using System.IO;


///<summary>
/// Implementations of everything that unity supports
/// Sub-classes override abstract methods that need platform-specific implementations
///</summary>
public abstract class Platform : SingletonBase
{
	protected double targetElapsedDistance = 0;

    // Services that probably don't need platform-specific overrides
	protected PlayerOrientation playerOrientation = new PlayerOrientation();
	
    // Listeners for unity messages, attached to Platform game object in GetMonoBehavioursPartner
    private BluetoothMessageListener _bluetoothMessageListener;
    public BluetoothMessageListener BluetoothMessageListener { get { return _bluetoothMessageListener; } }
    
    // internal platform tools
    public PlatformPartner partner;  // MonoBehavior that passes unity calls through to platform
    
    // internal platform state
    protected static bool applicationIsQuitting = false;
    protected bool initialised = false;
    private int sessionId = 0;
    public bool connected { get; protected set; }
    public float syncInterval = 60f;  // Other components may change this to disable sync temporarily?

    
	/// <summary>
	/// Gets the single instance of the right kind of platform for the OS we're running on,
	/// or creates one if it doesn't exist.
	/// </summary>
	/// <value>
	/// The instance.
	/// </value>
    public static Platform Instance 
    {
		get 
        {
#if UNITY_EDITOR
            return (Platform)GetInstance<PlatformDummy>();
#elif UNITY_ANDROID && RACEYOURSELF_MOBILE
            return (Platform)GetInstance<MinimalAndroidPlatform>();
#elif UNITY_ANDROID
            return (Platform)GetInstance<MinimalAndroidPlatform>();
#elif UNITY_IPHONE
            return (Platform)GetInstance<IosPlatform>();
#endif
            return null;
        }
    }
   
	public virtual void OnDestroy() {
		applicationIsQuitting = true;
	}

	public override void Awake()
    {
        base.Awake();

		if (initialised == false)
        {
            Initialize();
        }
    }

	protected virtual void Initialize()
	{
		connected = false;
	
		Input.location.Start(10,1);

		// Set initialised=true in overriden method
	}

	protected virtual void PostInit()
    {
		BluetoothServer();

		// start listening for 2-tap gestures to reset gyros
	/*	GestureHelper.onTwoTap += new GestureHelper.TwoFingerTap(() => {
            if (IsRemoteDisplay())
            {
                Platform.Instance.GetPlayerOrientation().Reset();
            }
            else
            {
                GUICamera[] scripts = GameObject.FindObjectsOfType(typeof(GUICamera)) as GUICamera[];
                foreach(GUICamera cameraScript in scripts)
                {
                    cameraScript.ResetCamera();
                }

            }
		});*/
	}


    /// <summary>
    /// Called every frame by PlatformPartner to update internal state
    /// </summary>
    public virtual void Update() {
        // overridden in subclasses to update orientation
	}   

    public void OnApplicationFocus(bool paused) {
        if (initialised && paused && OnGlass()) {
            Application.Quit();
        }
	}

    public void OnApplicationQuit ()
    {
        // nothing for now
    }

    public void SetMonoBehavioursPartner(PlatformPartner obj)
    {
        if (partner == null)
        {
            //named object to identify platform game object reprezentation
            //GameObject go = new GameObject("Platform");
            //partner = go.AddComponent<PlatformPartner>();
            _bluetoothMessageListener = obj.gameObject.AddComponent<BluetoothMessageListener>();

            //post initialziation procedure
            partner = obj;
            PostInit();
        }
    }

    public PlatformPartner GetMonoBehavioursPartner()
    {
        if (partner == null)
        {
            Debug.Log("Partner is not set yet. scene not constructed, you cant refer scene objects. Do you try to do so from another thread?");
        }

        return partner;
    }

    //***** Convenience methods that mostly return values from the database  ****

    // Get the device's orientation
    public virtual PlayerOrientation GetPlayerOrientation() {
        return playerOrientation;
    }

    // *** Methods that need platform-specific overrides ***

    public abstract bool OnGlass ();

    public abstract bool IsRemoteDisplay ();

    public abstract bool IsPluggedIn ();

    public abstract bool HasInternet ();

    public abstract bool HasWifi ();

    public abstract bool IsDisplayRemote ();

    public abstract bool IsBluetoothBonded ();

    public abstract byte[] LoadBlob (string id);

    public abstract void StoreBlob (string id, byte[] blob);

    public abstract void EraseBlob (string id);

    public abstract void ResetBlobs ();

    // Returns the int number of fingers touching glass's trackpad
    public abstract int GetTouchCount ();

    // Returns (x,y) as floats between 0 and 1
    public abstract Vector2? GetTouchInput ();

    public abstract void BluetoothServer ();

    public abstract void BluetoothClient ();

    public abstract void BluetoothBroadcast (string json);

    public abstract string[] BluetoothPeers ();


	public virtual byte[] ReadAssets(string filename) 
	{
		string assetspath = Application.streamingAssetsPath;
		if (assetspath.Contains("://")) {
			var www = new WWW(Path.Combine(assetspath, filename));
			while(!www.isDone) {}; // block until finished
			return www.bytes;
		} else {
			return File.ReadAllBytes(Path.Combine(assetspath, filename));			
		}
	}

	public string ReadAssetsString(string filename) 
	{
		return new System.Text.UTF8Encoding().GetString(ReadAssets(filename));
	}
}