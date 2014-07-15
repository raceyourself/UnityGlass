using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

/// <summary>
/// basic panel which allows to show ui
/// </summary>
[Serializable]
public class Panel : FlowState
{
    static public string[] InteractivePrefabs = { "UIComponents/Button",
												  "MainGUIComponents/ResetGyroButton",
												  "MainGUIComponents/SettingsButton",
												  "SettingsComponent/IndoorButton",
												  "Friend List/ChallengeButton"};    
    public FlowPanelComponent panelNodeData;
    [NonSerialized()] public GameObject physicalWidgetRoot;    

    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
	public Panel() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public Panel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
        this.panelNodeData = (FlowPanelComponent)info.GetValue("panelNodeData", typeof(FlowPanelComponent));
        if (this.panelNodeData != null)
        {
            this.panelNodeData.RefreshData();
        }
        else
        {
            int a = 0;
        }
        
    }

    /// <summary>
    /// serialization function called by serializer
    /// </summary>
    /// <param name="info">serialziation info where all data would be pushed to</param>
    /// <param name="ctxt">serialzation context</param>
    /// <returns></returns>
    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        base.GetObjectData(info, ctxt);
        info.AddValue("panelNodeData", this.panelNodeData);        
   	}

    /// <summary>
    /// Gets display name of the node, helps with node identification in editor
    /// </summary>
    /// <returns>name of the node</returns>
    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "Panel: " + gName.Value;
        }
        return "Panel: UnInitialzied";
    }

    /// <summary>
    /// initializes node and creates name for it. Makes as well default iput/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(250, 80);
        NewInput("Enter", "Flow");
       // NewOutput("Exit", "Flow");
        NewParameter("Type", GraphValueType.UIPrefab, ""); 
        NewParameter("Name", GraphValueType.String, "Set Panel Title");
        NewParameter("Settings", GraphValueType.Settings, "");        
    }

    /// <summary>
    /// refreshes connections lists
    /// </summary>
    /// <returns></returns>
    public override void RebuildConnections()
    {        
        if (Outputs != null) Outputs.Clear();        

        GParameter gType = Parameters.Find(r => r.Key == "Type");

        SerializedNode node = GetPanelSerializationNode(gType.Value);
        LookForInteractiveItems(node);

        int count = Mathf.Max(Inputs.Count, Outputs.Count);        

        RefreshNodeData();
        base.RebuildConnections();   
    }

    /// <summary>
    /// builds flow panel component data based on serialized nodes based on panel specified name stored as variable "Type" on parameter list
    /// </summary>
    /// <returns></returns>
    public void RefreshNodeData()
    {
        GParameter gType = Parameters.Find(r => r.Key == "Type");

        panelNodeData = new FlowPanelComponent(GetPanelSerializationNode(gType.Value));
    }

    /// <summary>
    /// finds buttons on current panel 
    /// </summary>
    /// <param name="node">root node</param>
    /// <returns></returns>
    private void LookForInteractiveItems(SerializedNode node)
    {
        if (node == null) return;

        bool found = false;

        foreach (string s in InteractivePrefabs)
        {
            if (node.GetPrefabName() == s)
            {                                
                NewOutput(node.GetName(),"Flow");
                found = true;
            }
        }

        if (!found)
        {
            GameObject prefab = Resources.Load(node.GetPrefabName()) as GameObject;
            if (prefab != null)
            {
                UIButton[] buttons = prefab.GetComponentsInChildren<UIButton>(true) as UIButton[];
                foreach (UIButton button in buttons)
                {
                    NewOutput(button.transform.parent.name, "Flow");
                }
            }
        }


        for (int i = 0; i < node.subBranches.Count; i++)
        {
            LookForInteractiveItems(node.subBranches[i]);
        }
    }

    /// <summary>
    /// finds all serializable settings on serializable node
    /// </summary>
    /// <param name="sn">serialziable node source to be analized for serialziable components</param>
    /// <returns></returns>
    private void CreateCustomizationParams(SerializedNode sn)
    {
        SerializableSettings ss = sn != null ? sn.GetSerializableSettings() : null;        
    }

    /// <summary>
    /// finds saved serialized structure and returns root of it
    /// </summary>
    /// <param name="selectedName">lookup name of the screen</param>
    /// <returns>screen serialized root</returns>
    public SerializedNode GetPanelSerializationNode(string selectedName)
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.ui_panels);
        if (s == null || s.dictionary == null)
        {           
            return null;
        }

        StorageDictionary screens = Panel.GetPanelDictionary();
        return screens != null ? screens.Get(selectedName) as SerializedNode : null;
    }

    /// <summary>
    /// function called when screen started enter process
    /// </summary>
    /// <returns></returns>
    public override void EnterStart()
    {
        base.EnterStart();

        UIManager script = (UIManager)GameObject.FindObjectOfType(typeof(UIManager));        
        StorageDictionary screensDictionary = Panel.GetPanelDictionary();

        if (script == null)
        {
            Debug.LogError("Scene requires to have UIManager in its root");
        }       
        else if (screensDictionary == null)
        {
            Debug.LogError("Scene requires to have screensDictionary which cant be found");
        }
        else
        {
            GParameter gType = Parameters.Find(r => r.Key == "Type");

			physicalWidgetRoot = script.LoadPrefabPanel(gType.Value, GetWidgetRootName());


            ISerializable data = screensDictionary.Get(gType.Value);
            if (data != null)
            {
                GParameter gName = Parameters.Find(r => r.Key == "Name");
                
				if (physicalWidgetRoot == null)
				{
					physicalWidgetRoot = script.LoadScene((SerializedNode)data, GetWidgetRootName(), panelNodeData);
				}
                              
            }

			if (physicalWidgetRoot != null)
			{
				physicalWidgetRoot.name = GetWidgetRootName() + "_" + gType.Value;
				Debug.Log("Name " + physicalWidgetRoot.name);
			}  

            if (physicalWidgetRoot != null)
            {
                Component[] buttons = physicalWidgetRoot.GetComponentsInChildren(typeof(UIButtonColor), true);
                if (buttons != null && buttons.Length > 0)
                {
                    foreach (UIButtonColor bScript in buttons)
                    {
                        FlowButton fb = bScript.GetComponent<FlowButton>();
                        if (fb == null)
                        {
                            fb = bScript.gameObject.AddComponent<FlowButton>();
                        }

                        fb.owner = this;
                        fb.name = fb.transform.parent.name;
                    }
                }
            }
        }
    }

    /// <summary>
    /// exit finalization and clearing process
    /// </summary>
    /// <returns></returns>
    public override void Exited()
    {
        base.Exited();

        UIManager script = (UIManager)GameObject.FindObjectOfType(typeof(UIManager));
        if (physicalWidgetRoot != null)
        {
            GameObject.Destroy(physicalWidgetRoot);        
        }        
    }

    /// <summary>
    /// whenever button get clicked it would be handled here
    /// </summary>
    /// <param name="button">button which send this event</param>
    /// <returns></returns>
    public virtual void OnClick(FlowButton button)
    {        
        if (Outputs.Count > 0 && parentMachine != null)
        {
            GConnector gConect = Outputs.Find(r => r.Name == button.name);
            
            if (gConect != null)
            {            
                ConnectionWithCall(gConect, button);                
            }            
        }
        else
        {
            Debug.LogError("Dead end");
        }
    }    

    /// <summary>
    /// buttons pressed or released on this panel would send events here
    /// </summary>
    /// <param name="button">button which send event</param>
    /// <param name="isDown">is it event on press down ? </param>
    /// <returns></returns>
    public virtual void OnPress(FlowButton button, bool isDown)
    {

    }

    /// <summary>
    /// function allowing to back from the panel to previously visited one
    /// </summary>
    /// <returns></returns>
    public virtual void OnBack()
    {
        parentMachine.FollowBack();
    }

    

    /// <summary>
    /// checks if class have button data and at least one button. As well do the parent checks if screen type is set
    /// </summary>
    /// <returns></returns>
    public override bool IsValid()
    {
        GParameter gType = Parameters.Find(r => r.Key == "Type");

        return base.IsValid() && gType != null && gType.Value != null && gType.Value != "Null";
    }
    

    /// <summary>
    /// Provides name of the widget, by default it is a widget of 2d graphics
    /// </summary>
    /// <returns>const name widget root name</returns>
    public virtual string GetWidgetRootName()
    {
        return "Widgets Container";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static StorageDictionary GetPanelDictionary()
    {
        return GetPanelDictionary(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tryOldOne"></param>
    /// <returns></returns>
    public static StorageDictionary GetPanelDictionary(bool tryOldOne)
    {
        Storage s = DataStore.GetStorage(DataStore.BlobNames.ui_panels);
        StorageDictionary screensDictionary = null;
        if (tryOldOne)
        {
            screensDictionary = (StorageDictionary)s.dictionary.Get(UIManager.UIPannels);
        }


        if (screensDictionary == null)
        {
            screensDictionary = s.dictionary;
        }

        return screensDictionary;
    }
}
