using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// start state which is simply forwarder to first node in the flow
/// </summary>
[Serializable]
public class GamePlayManagerState : FlowState
{
    private GameObject model;
    //private int lastTimeStamp;
   // private float speed;

    private Vector3 target;
    private float speed;
    
    //private 

    /// <summary>
    /// default constructor
    /// </summary>
	public GamePlayManagerState() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public GamePlayManagerState(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
    }

    
    /// <summary>
    /// initialzies node and creates name for it. Makes as well default iput/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(175, 80);
        NewInput("Enter", "Flow");
        NewOutput("GameFinished", "Flow");
    }

    public override void Entered()
    {
        base.Entered();

        model = GameObject.Instantiate(Resources.Load("David")) as GameObject;

        Animator a = model.GetComponent<Animator>();
        a.SetBool("Running", true);

        DataVault.RegisterListner(this, "player_ahead_value");
        DataVault.RegisterListner(this, "finish_race");
    }

    public override void StateUpdate()
    {        
        base.StateUpdate();

        if (model != null)
        {
            Vector3 pos = model.transform.position;
            model.transform.position = pos + new Vector3(0, 0, speed);
        }
    }

    public override void Apply(string identifier)
    {
        switch (identifier)
        {
            case "player_ahead_value":
                Debug.Log("Apply for player_ahead_value");
                object value = DataVault.Get("player_ahead_value");
                if (model != null && (value is float))
                {
                    Debug.Log("set model position! with "+value);
                    Vector3 newTarget = new Vector3(0, 0, -((float)value));

                    //interpolate over 30 frames
                    speed = (newTarget - target).z / 30.0f;
                }
                break;

            case BluetoothMessageListener.MESSAGE_FINISH_RACE:
                bool finish = DataVault.GetBool(BluetoothMessageListener.MESSAGE_FINISH_RACE);
                if (finish)
                {
                    DataVault.Set("finish_race", false);
                    FollowFlowLinkNamed("GameFinished");
                }
                break;
        }
    }

   
}
