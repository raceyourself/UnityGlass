using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


/// <summary>
/// Every function in collection have to accept FlowButton and panel variable and return boolean helping to decide if navigation should continue or stop
/// </summary>
public class ButtonFunctionCollection
{

	/// <summary>
	/// default testing function 
	/// </summary>
	/// <param name="fb"> button providng event </param>
	/// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
	/// <returns> Is button in state to continue? If False is returned button will not navigate forward on its own connection!</returns>
	static public bool MyFunction1(FlowButton fb, FlowState panel)
	{
		Debug.Log("Testing linked function true");
      
		return true;
	}

    static public bool IsTutorialRequired(FlowButton button, Panel panel)
    {
        object o = DataVault.Get("tutorial_done");
        if (o != null)
        {
            if (Convert.ToBoolean(o))
            {
                panel.ConnectionWithCall("ToGame");
                return false;
            }
        }

        return true;
    }

    static public bool SetTutorialDone(FlowButton button, Panel panel)
    {
        DataVault.Set("tutorial_done", true);
        DataVault.SetPersistency("tutorial_done", true);
        DataVault.SaveToBlob();

        return true;
    }

}
