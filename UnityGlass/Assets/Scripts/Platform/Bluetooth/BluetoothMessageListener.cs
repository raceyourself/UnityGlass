using System;
using UnityEngine;
using SimpleJSON;

//this class can be used only if exists on gameobject named "Platform"

//units are metres for distance, metres/second for speed, and milliseconds for time
public class BluetoothMessageListener : MonoBehaviour
{
    #region position_update
    const string PLAYER_DATA    = "player_data";
    const string OPPONENT_DATA  = "opponent_data";

    const string CALORIES       = "calories";
    const string AVERAGE_SPEED  = "average_speed";
    const string DISTANCE       = "distance";
    const string ELAPSED_TIME   = "elapsed_time";
    const string CURRENT_SPEED  = "current_speed";
    const string AHEAD_BEHIND   = "ahead_behind";    
    #endregion

    enum Type
    {
        none,
        distance,
        speed,
        time
    }

    void Awake()
    {
        if (gameObject.name != "Platform")
        {
            Debug.LogError("ERROR!: BluetoothMessageListener have to be attached to game object named 'Platform'");
        }
    }

    protected void OnBluetoothJson(JSONNode json) {
        UnityEngine.Debug.Log("Platform: OnBluetoothJson ");
       
        //test debug data:
        UnityEngine.Debug.Log("Platform: Bluetooth message: " + json.ToString());        

        switch(json["action"]) 
        {
            case "position_update":                
                bool playerIsWinning = true;                                            
                string avs = SetDataVaultInt("player", PLAYER_DATA, AVERAGE_SPEED   , json, Type.speed);
                string dis = SetDataVaultInt("player", PLAYER_DATA, DISTANCE        , json, Type.distance);
                string elt = SetDataVaultInt("player", PLAYER_DATA, ELAPSED_TIME    , json, Type.time);
                string cus = SetDataVaultInt("player", PLAYER_DATA, CURRENT_SPEED   , json, Type.speed);
                string ahb = SetDataVaultInt("player", PLAYER_DATA, AHEAD_BEHIND    , json, Type.distance, out playerIsWinning);
                             SetDataVaultInt("player", PLAYER_DATA, CALORIES        , json, Type.none);

                
                DataVault.Set(AVERAGE_SPEED , "AVERAGE SPEED("+avs+")");
                DataVault.Set(DISTANCE      , "DISTANCE(" + dis + ")");
                DataVault.Set(ELAPSED_TIME  , "ELAPSED TIME(" + elt + ")");
                DataVault.Set(CURRENT_SPEED ,  cus );                
                DataVault.Set(CALORIES      , "CALORIES(kcal)");

                if (playerIsWinning)
                {
                    DataVault.Set(AHEAD_BEHIND, ahb + " AHEAD");
                }
                else
                {
                    DataVault.Set(AHEAD_BEHIND, ahb + " BEHIND");
                }

                break;

            default:
                UnityEngine.Debug.Log("Platform: unknown Bluetooth message: " + json.ToString());                
                break;
        }        
    }

    public void OnBluetoothConnect(string message) {
    }

    public void OnBluetoothMessage(string message) {
        //      MessageWidget.AddMessage("Bluetooth", message, "settings"); // DEBUG
        UnityEngine.Debug.Log("Platform: OnBluetoothMessage " + message.Length + "B"); 
        JSONNode json = JSON.Parse(message);
        OnBluetoothJson(json);
    }

    static public int ReadAsInt(object data)
    {        
        try
        {
            int val = Convert.ToInt32(data);
            return val;
        }
        catch
        {
            return -1;
        }        
    }

    static private string SetDataVaultInt(string prefix, string rootPoint, string dataName, JSONNode json, Type type)
    {
        bool isNeg;
        return SetDataVaultInt(prefix, rootPoint, dataName, json, type, out isNeg);
    }

    static private string SetDataVaultInt(string prefix, string rootPoint, string dataName, JSONNode json, Type type, out bool isNeg)
    {
        int value = ReadAsInt(json[rootPoint][dataName]);        
        if (value < 0)
        {
            isNeg = true;
            value = -value;
        }
        else
        {
            isNeg = false;
        }

        string unit = "";
        string sValue = "";
        float v;

        switch (type)
        {
            case Type.distance:
                if (value >= 1000)
                {
                    v = ((float)value) / 1000.0f;
                    sValue = v.ToString("G3");
                    unit = "km";
                }
                else
                {
                    sValue = value.ToString();
                    unit = "m";
                }
                break;

            case Type.speed:
                v = ((float)value) / 3600.0f;
                sValue = v > 0.1f ? v.ToString("G3") : "0";
                unit = "km/h";                
                break;

            case Type.time:

                if (value < 60000)
                {
                    sValue = (int)(value / 1000) + " ";
                    unit = "sec";                
                }
                else if (value < 60*60000)
                {
                    int sec = (int)(value / 1000) % 60;
                    int min = (int)(value / 60000);
                    sValue = min+ ":"+sec.ToString("D2");
                    unit = "min:sec";                
                }
                else
                {
                    int min = (int)(value / 60000) % 60;
                    int hr = (int)(value / (60*60000));
                    sValue = hr + ":" + min.ToString("D2");
                    unit = "hr:min";                
                }
                break;

            default:
                sValue = value.ToString();
                unit = "";
                break;
        }
        if (isNeg)
        {
            DataVault.Set(prefix + "_" + dataName, "-"+sValue);
        }
        else
        {
            DataVault.Set(prefix + "_" + dataName, sValue);
        }
        
        return unit;
    }
}

