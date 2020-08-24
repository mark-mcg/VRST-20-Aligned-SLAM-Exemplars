using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OVRExecutorEvent : ExecutorEvent
{
    public OVRInput.Button button;
    public OVRInput.Controller controller;

    public override bool ActionOnset()
    {
        return OVRInput.GetDown(button, controller);
    }

    public override bool ActionContinuing()
    {
        return OVRInput.Get(button, controller);
    }

    public override string ToString()
    {
        return "OVRExecutorEvent for button " + button + " controller" + controller;
    }
}