using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// start state which is simply forwarder to first node in the flow
/// </summary>
[Serializable]
public class BTEventListener : FlowState 
{
    /// <summary>
    /// default constructor
    /// </summary>
	public BTEventListener() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public BTEventListener(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
    }

    protected override void Initialize()
    {
        base.Initialize();

        NewInput("Enter", "Flow");
    }

    public override void EnterStart()
    {
        base.EnterStart();

        DataVault.RegisterListner(this, BluetoothMessageListener.MESSAGE_NEW_BT_MESSAGE);
    }

    public override void Apply(string identifier)
    {
        base.Apply(identifier);

        string message = DataVault.GetString(BluetoothMessageListener.MESSAGE_NEW_BT_MESSAGE);

        FollowFlowLinkNamed(message, this);
    }

}
