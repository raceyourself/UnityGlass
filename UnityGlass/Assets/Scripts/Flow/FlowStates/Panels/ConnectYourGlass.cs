using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class ConnectYourGlass : Panel 
{	
	public ConnectYourGlass() {}
    public ConnectYourGlass(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

    protected override void Initialize()
    {
        base.Initialize();
        
        NewOutput("Exit", "Flow");        
    }

    public override void EnterStart()
    {
        base.EnterStart();
        
        DataVault.RegisterListner(this, BluetoothMessageListener.MESSAGE_BT_CONNECTED);
        Apply(BluetoothMessageListener.MESSAGE_BT_CONNECTED);
    }

    public override void Apply(string identifier)
    {
        base.Apply(identifier);

        bool value = DataVault.GetBool(BluetoothMessageListener.MESSAGE_BT_CONNECTED);

        Debug.Log("Received apply with "+value +" for ConnectYourGlass ");

        if (value)
        {
            FollowFlowLinkNamed("Exit");
        }
    }
}
