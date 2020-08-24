using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionEventPair<EventType, ActionType>
    where ActionType : ExecutorAction
    where EventType : ExecutorEvent
{
    public EventType listenEvent;
    public ActionType eventAction;

    public override string ToString()
    {
        return "Action " + eventAction + " listenEvent " + listenEvent; 
    }
}