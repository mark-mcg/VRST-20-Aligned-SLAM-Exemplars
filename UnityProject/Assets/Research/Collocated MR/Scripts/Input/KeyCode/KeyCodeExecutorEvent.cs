using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class KeyCodeExecutorEvent : ExecutorEvent
{
    public KeyCode key;

    public override bool ActionOnset()
    {
        return Input.GetKeyDown(key);
    }

    public override bool ActionContinuing()
    {
        return Input.GetKey(key);
    }

    public override string ToString()
    {
        return "KeyCodeExecutorEvent for key " + key;
    }
}