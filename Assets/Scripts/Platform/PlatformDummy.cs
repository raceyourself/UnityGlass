using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

#if (UNITY_EDITOR || RACEYOURSELF_MOBILE)
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteInEditMode()] 
#endif
public class PlatformDummy : Platform
{
	const string STARTHEX_SCENE_NAME = "Assets/Scenes/Start Hex.unity";
	const string SNACKRUN_SCENE_NAME = "Assets/Scenes/SnackRun.unity";
	const string UITEST_SCENE_NAME = "Assets/Scenes/UITestScene.unity";

	// Helper class for accessing the player's current position, speed and direction of movement
	private PlayerPosition _localPlayerPosition;
    public override PlayerPosition LocalPlayerPosition {
        get { return _localPlayerPosition; }
    }

	private string blobstore = "game-blob";
	private string blobassets = "blob";

	private float oriYaw = 0.0f;
	private float oriPitch = 0.0f;
	const float lookSensitivity = 1.0f;

    public override bool OnGlass()
    {
        return true;
    }
	public override bool IsRemoteDisplay()
	{
		return true;
	}
    public override bool IsPluggedIn() {
        // TODO given HasWifi, can this safely be set to true?
        return Application.isPlaying;
	}
	public override bool HasInternet() {
		return true;
	}	
	public override bool HasWifi() {
        // we don't want code that is used when navigating the editor to trigger syncs outside of play mode!
        return Application.isPlaying;
	}	
	public override bool IsDisplayRemote() {
		return false;
	}	
	public override bool HasGpsProvider() {
		return false;
	}	
    public override bool IsBluetoothBonded()
    {
        return false;
    }

#if UNITY_EDITOR
	[MenuItem("Race Yourself/Play from StartHex Scene, with flow at Start %0")]
	public static void PlayFromStartHex()
    {

		PlayWithScene(STARTHEX_SCENE_NAME, false, false);
	}

	[MenuItem("Race Yourself/Play from current Scene, with flow at Game Intro %[")]
	public static void PlayFromCurrentGameScene()
	{
		PlayWithScene(null, true, false);
	}

	[MenuItem("Race Yourself/Play from SnackRun Scene, with flow at Game Intro %]")]
	public static void PlayFromSnackRunscene()
	{
		PlayWithScene(SNACKRUN_SCENE_NAME, true, false);
	}

	[MenuItem("Race Yourself/Play with Mobile UX %.")]
	public static void PlayWithMobileUX()
	{
		PlayWithScene(STARTHEX_SCENE_NAME, false, true);
	}
	
	[MenuItem("Race Yourself/Load UITestScene %u")]
	public static void LoadUITestScene()
	{
		EditorApplication.SaveCurrentSceneIfUserWantsTo();
		EditorApplication.OpenScene(UITEST_SCENE_NAME);
	}
	
	protected static void PlayWithScene(string scene, bool toGame, bool toMobile)
	{
		//encode the 'intent' in this integer
		if(toGame)
		{
			PlayerPrefs.SetInt("toGame", 1);
		}
		else if(toMobile)
		{
			PlayerPrefs.SetInt("toGame", 2);
		}
		else
		{
			PlayerPrefs.SetInt("toGame", 0);
		}

		//load scene, then play
		if(scene != null)
		{
			EditorApplication.SaveCurrentSceneIfUserWantsTo();
			EditorApplication.OpenScene(scene);
		}
		//play
		EditorApplication.isPlaying = true;

		//initialise objects for flow, datastorage, UIScene etc, if this isn't startHex
		if(scene != STARTHEX_SCENE_NAME)
		{
			InitForPreview playModePreparer = (InitForPreview)GameObject.FindObjectOfType(typeof(InitForPreview));
			if(playModePreparer != null)
			{
				playModePreparer.PrepareForPlayMode();
			}
			else
			{
				UnityEngine.Debug.LogError("PlatformDummy: Unable to initialise correctly for Play mode in editor. Ensure that a correctly configured InitForPreview component is present in the scene");
			}
		}

	}
#endif

	protected override void Initialize()
	{
		try {
			initialised = false;

			UnityEngine.Debug.Log("Creating Platform Dummy instance");

			// blobstore init moved to getBlob

			if (!initialised) {
				playerOrientation.Update(Quaternion.FromToRotation(Vector3.down,Vector3.forward));

				initialised = true;
			} else {
				UnityEngine.Debug.Log("Race condition in PlatformDummy!");
			}
	    } catch (Exception e) {
            UnityEngine.Debug.LogWarning("Platform: Error in constructor " + e.Message);
            UnityEngine.Debug.LogException(e);
			Application.Quit();
	    }
	}

	

	public override PlayerOrientation GetPlayerOrientation() {
		return playerOrientation;
	}

	public override byte[] LoadBlob(string id) {
		try {
			UnityEngine.Debug.Log("PlatformDummy: Loading blob id: " + id);			
			return File.ReadAllBytes(Path.Combine(getBlobStorePath(), id));			
		} catch (FileNotFoundException e) {
			return LoadDefaultBlob(id);
		}
	}

	public byte[] LoadDefaultBlob(string id) {
		try {
			UnityEngine.Debug.Log("PlatformDummy: Loading default blob id: " + id);
			if (blobassets.Contains("://")) {
				var www = new WWW(Path.Combine(blobassets, id));
				while(!www.isDone) {}; // block until finished
				return www.bytes;
			} else {
				return File.ReadAllBytes(Path.Combine(blobassets, id));			
			}
		} catch (FileNotFoundException e) {
			return new byte[0];
		}
	}

	protected string getBlobStorePath()
	{
        // TODO - this ought to be done just once in initialize, but seems to get called before initialize completes
        blobstore = Path.Combine(Application.persistentDataPath, blobstore);
        blobassets = Path.Combine(Application.streamingAssetsPath, blobassets);
        var tag = "Player";
        if (!Application.isPlaying) {
            // Save to blob assets in editor
            blobstore = blobassets;
            tag = "Editor";
        }
        Directory.CreateDirectory(blobstore);
        UnityEngine.Debug.Log(tag + " blobstore: " + blobstore);
        if (Application.isEditor) Directory.CreateDirectory(blobassets);
        UnityEngine.Debug.Log(tag + " blobassets: " + blobassets);

        if(Application.isEditor)// && !Application.isPlaying)
		{
			//modify the actual assets directly in the editor
			return blobassets;
		}
		else
		{
			//Use the 'store' - a writable copy for the play mode session
			return blobstore;
		}
	}

    public override void StoreBlob(string id, byte[] blob)
    {
        File.WriteAllBytes(Path.Combine(getBlobStorePath(), id), blob);
		UnityEngine.Debug.Log("PlatformDummy: Stored blob id: " + id + "to path: " + blobstore);
    }

	public override void ResetBlobs ()
	{
		//Not entirely sure what this is supposed to do. Wil do nothing for now. AH
		return;
	}

    public override void EraseBlob (String id)
    {
        throw new NotImplementedException ();
    }

	public override void BluetoothClient ()
	{
	}

	public override void BluetoothServer ()
	{
	}

	public override string[] BluetoothPeers ()
	{
		return new string[0];
	}

	public override void BluetoothBroadcast (string json)
	{
		try {
			SimpleJSON.JSON.Parse(json);
			log.info("BluetoothBroadcast: " + json);		
		} catch (Exception e) {
			log.error(e, "BluetoothBroadcast: could not parse json!");
		}
		return;
	}



	public override void Update ()
	{	
		base.Update();		
		if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			//check for input and update player orientation as appropriate
			float x = Input.GetAxis("Mouse X");
			float y = Input.GetAxis("Mouse Y");

			oriYaw -= x * lookSensitivity;
			oriPitch += y * lookSensitivity;

			//construct quaternion and update player ori
			Quaternion fromForward = Quaternion.Euler(oriPitch, oriYaw, 0.0f);
			Quaternion fromDown = Quaternion.FromToRotation(Vector3.down,Vector3.forward) * fromForward;

			playerOrientation.Update(fromDown);
		}
	}

	public override Vector2? GetTouchInput ()
	{
		if(GetTouchCount() > 0)
		{
			float x = Input.mousePosition.x / Screen.width;
			float y = Input.mousePosition.y / Screen.height;
			return new Vector2(x,y);
		}
		else return null;
	}

	public override int GetTouchCount ()
	{
		int touchCount = 0;
		//simulate multiple touchers by holding more than one modifier key
		if(Input.GetKey(KeyCode.LeftControl)) { touchCount++;}
		if(Input.GetKey(KeyCode.LeftCommand)) { touchCount++;}
		if(Input.GetKey(KeyCode.RightControl)) { touchCount++;}
		if(Input.GetKey(KeyCode.RightCommand)) { touchCount++;}
		return touchCount;
	} 

}