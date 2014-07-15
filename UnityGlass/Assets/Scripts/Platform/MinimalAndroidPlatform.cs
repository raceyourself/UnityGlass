using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sqo;

#if UNITY_ANDROID
/// <summary>
/// Android platform. Overrides platform functionality with android-specific functionality where necessary. Usually this means JNI calls to the GlassfitPlatform libr
/// </summary>
public class MinimalAndroidPlatform : Platform
{

    // Native android class/object references
    private AndroidJavaClass build_class;
    private AndroidJavaObject helper;
    private AndroidJavaClass helper_class;
    private AndroidJavaObject activity;
    private AndroidJavaObject context;


    protected override void Initialize() {
        log.info("Initialising MinimalAndroidPlatform");
        base.Initialize();
        try {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            context = activity.Call<AndroidJavaObject>("getApplicationContext");
            helper_class = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");
            build_class = new AndroidJavaClass("android.os.Build");

            // call the following on the UI thread
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {

                try {
                    // Get the singleton helper objects
                    helper = helper_class.CallStatic<AndroidJavaObject>("getInstance", context);

                    // Log screen dimensions - for debug only, can be commented out
                    log.info("Screen dimensions are " + GetScreenDimensions().x.ToString() + "x" + GetScreenDimensions().y.ToString());
                } catch (Exception e) {
                    log.error(e, "JNI error in android initialisation UI thread");
                    Application.Quit();
                }
                initialised = true;
                log.info("Initialise complete");
            }));
			
            log.info("Native android classes created OK");

        } catch (Exception e) {
            log.error(e, "JNI error in android initialisation main thread");
            Application.Quit();
        }       
    }

    /// <summary>
    /// Called every frame by PlatformPartner to update internal state
    /// </summary>
    public override void Update() {
        base.Update();
        try {
//            log.info("Getting orientation over JNI");
            AndroidJavaObject q = helper.Call<AndroidJavaObject>("getOrientation");
            playerOrientation.Update(new Quaternion(q.Call<float>("getX"), q.Call<float>("getY"), q.Call<float>("getZ"), q.Call<float>("getW")));
        } catch (Exception e) {
            log.error(e, "JNI error getting orientation");
        }
    }
	
	public override bool OnGlass() 
    {
        try {
            //log.info("seeing if glass");
            return helper_class.CallStatic<bool>("onGlass");
        } catch (Exception e) {
            log.error(e, "JNI onGlass() failed");
            return false;
        }
    }
	
	public override bool IsRemoteDisplay() 
    {
        try {
            //log.info("seeing if glass");
            return helper_class.CallStatic<bool>("isRemoteDisplay");
        } catch (Exception e) {
            log.error(e, "JNI IsRemoteDisplay() failed");
            return false;
        }
    }

    public override bool IsPluggedIn()
    {
        try {
            log.info("Calling IsPluggedIn");
            return helper.Call<bool>("isPluggedIn");
        } catch (Exception e) {
            log.error(e, "JNI IsPluggedIn() failed");
            return false;
        }
    }

    public override bool HasInternet() {
        try {
            return helper.Call<bool>("hasInternet");
        } catch (Exception e) {
            log.error(e, "JNI HasInternet() failed");
            return false;
        }
    }

    public override bool HasWifi() {
        try {
            return helper.Call<bool>("hasWifi");
        } catch (Exception e) {
            log.error(e, "JNI HasWifi() failed");
            return false;
        }
    }

    public override bool IsDisplayRemote() {
        foreach(string peer in BluetoothPeers()) {
            log.info("BT peer: " + peer);
            if (peer.Contains("Glass") || peer.Contains("Display")) return true;
        }
        return false;
    }

    public override bool IsBluetoothBonded()
    {
        try {
            return helper.Call<bool>("isBluetoothBonded");
        } catch (Exception e) {
            log.error(e, "JNI IsBluetoothBonded() failed");
            return false;
        }
    }

    private Vector2i GetScreenDimensions()
    {
        try
        {
            AndroidJavaObject displayMetrics = context.Call<AndroidJavaObject>("getResources").Call<AndroidJavaObject>("getDisplayMetrics");
            int height = displayMetrics.Get<int>("heightPixels");
            int width = displayMetrics.Get<int>("widthPixels");
            return new Vector2i(width, height);
        }
        catch (Exception e)
        {
            log.error(e, "JNI GetScreenDimensions() failed");
            return new Vector2i(0,0);
        }
    } 



    // *** blob storage, will probably move to database ***

    // Load the game blob
    public override byte[] LoadBlob(string id) {
        try {
            byte[] blob = helper_class.CallStatic<byte[]>("loadBlob", id);
            log.info("Game blob " + id + " of size: " + blob.Length + " loaded");
            return blob;
        } catch (Exception e) {
            log.error(e, "JNI LoadBlob() failed");          
        }
        return null;
    }

    // Store the blob
    public override void StoreBlob(string id, byte[] blob) {
        try {
            log.info("storing blob");
            helper_class.CallStatic("storeBlob", id, blob);
            log.info("Game blob " + id + " of size: " + blob.Length + " stored");
        } catch (Exception e) {
            log.error(e, "JNI StoreBlob() failed");
        }
    }

    // Update the data
    public override void EraseBlob(string id) {
        try {
            helper_class.CallStatic("eraseBlob", id);
            log.info("Game blob " + id + " erased");
        } catch (Exception e) {
            log.error(e, "JNI EraseBlob() failed");          
        }
    }

    public override void ResetBlobs() {
        try {
            helper_class.CallStatic("resetBlobs");
            log.info("Game blobs reset");
        } catch (Exception e) {
            log.error(e, "JNI ResetBlobs() failed");          
        }
    }

    // Poll java for touch input
    // Returns the int number of fingers touching the pad
    public override int GetTouchCount()
    {
        if (OnGlass ()) {
            try
            {
                return activity.Call<int> ("getTouchCount");
            }
            catch(Exception e)
            {
                log.error(e, "JNI GetTouchCount() failed");
                return 0;
            }
        } else {
            //use unity's built-in input for now
            return Input.touchCount;
        }
    }

    // Poll java for touch input
    // Returns (x,y) as floats between 0 and 1
    public override Vector2? GetTouchInput()
    {
        if (OnGlass ()) {
            try
            {
                //log.info("Checking touch input..");
                int touchCount = activity.Call<int> ("getTouchCount");
                if (touchCount > 0)
                {
                    float x = 1 - activity.Call<float> ("getTouchX");  // glass swipe forward === tablet swipe left
                    float y = activity.Call<float> ("getTouchY");
                    return new Vector2(x,y);
                } else {
                    return null;
                }
            }
            catch(Exception e)
            {
                log.error(e, "JNI GetTouchInput() failed");
                return null;
            }
        } else {
            //use unity's built-in input for now
            if (Input.touchCount == 1)
            {
                float x = Input.touches[0].position.x / Screen.width;
                float y = Input.touches[0].position.y / Screen.height;
                return new Vector2(x,y);
            }
            else
            {
                return null;
            }
        }
    }



    // *** Bluetooth ***

    public override void BluetoothServer()
    {
        try
        {
            activity.Call("startBluetoothServer");
            log.info("Starting Bluetooth server");
        }
        catch (Exception e)
        {
            log.error(e, "JNI BluetoothServer() failed");
        }
    }

    public override void BluetoothClient()
    {
        try
        {
            activity.Call("startBluetoothClient");
            log.info("Starting Bluetooth client");
        }
        catch (Exception e)
        {
            log.error(e, "JNI BluetoothClient() failed");
        }
    }

    public override void BluetoothBroadcast(string json) {
        try
        {
            activity.Call("broadcast", json);
            log.info("Broadcasted Bluetooth message: " + json.ToString());
        }
        catch (Exception e)
        {
            log.error(e, "JNI BluetoothBroadcast() failed");
        }
    }

    public override string[] BluetoothPeers() {
        try
        {
            return activity.Call<string[]>("getBluetoothPeers");
        }
        catch (Exception e)
        {
            log.error(e, "JNI BluetoothPeers() failed");
            return new string[0];
        }
    }

}
#endif