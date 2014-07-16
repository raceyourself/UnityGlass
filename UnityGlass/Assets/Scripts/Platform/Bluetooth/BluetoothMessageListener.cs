using System;
using UnityEngine;
using SimpleJSON;

public class BluetoothMessageListener : MonoBehaviour
{
    protected void OnBluetoothJson(JSONNode json) {
        UnityEngine.Debug.Log("Platform: OnBluetoothJson"); 
        switch(json["action"]) 
        {

        default:
            UnityEngine.Debug.Log("Platform: unknown Bluetooth message: " + json);
            break;
        }
            
        // TODO: Start challenge
        // TODO: Toggle outdoor/indoor
    }

    public void OnBluetoothConnect(string message) {
    }

    public void OnBluetoothMessage(string message) {
        //      MessageWidget.AddMessage("Bluetooth", message, "settings"); // DEBUG
        UnityEngine.Debug.Log("Platform: OnBluetoothMessage " + message.Length + "B"); 
        JSONNode json = JSON.Parse(message);
        OnBluetoothJson(json);
    }
}

