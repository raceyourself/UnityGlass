using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class PausePanel : TapSwipePanel {
	
    static protected string screenInputLock = "Pause";
    static protected string dataValultForcedPause = "force_pause_game";

	public PausePanel() {}
    public PausePanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

    public override void EnterStart()
    {
        base.EnterStart();

        physicalWidgetRoot.SetActive(false);

        DataVault.RegisterListner(this, dataValultForcedPause);
        Apply(dataValultForcedPause);
    }

    public override void Apply(string identifier)
    {
        base.Apply(identifier);

        bool value = DataVault.GetBool(dataValultForcedPause);
        ShowGraphic(value);        
    }

    private void ShowGraphic(bool visible)
    {
        if (visible == false && physicalWidgetRoot.activeSelf == true)
        {
            physicalWidgetRoot.SetActive(false);
            DataVault.Set(LOCK_NAME, tempLock);
            Platform.Instance.BluetoothActionBroadcast("pause", false);

        }
        else if (visible == true && physicalWidgetRoot.activeSelf == false)
        {
            physicalWidgetRoot.SetActive(true);
            DataVault.Set(LOCK_NAME, screenInputLock);
            Platform.Instance.BluetoothActionBroadcast("pause", true);
        }
    }

    public override void DefineHandlers()
    {
        tapHandler = new GestureHelper.OnTap(() => 
        {
            if (DataVault.GetString(LOCK_NAME) != string.Empty &&
                DataVault.GetString(LOCK_NAME) != screenInputLock)
            {
                return;
            }

            ShowGraphic(false);
		});

        downHandler = new GestureHelper.DownSwipe(() =>
        {
            if (DataVault.GetString(LOCK_NAME) != string.Empty &&
                DataVault.GetString(LOCK_NAME) != screenInputLock)
            {
                return;
            }

            if (physicalWidgetRoot.activeSelf != true)
            {
                ShowGraphic(true);
            }
            else
            {
                if (Outputs.Count > 0 && parentMachine != null)
                {
                    GConnector gConect = Outputs.Find(r => r.Name == "OnSwipeDown");
                    if (gConect != null)
                    {
                        ConnectionWithCall(gConect, null);
                    }
                }
            }
        });	
    }

}
