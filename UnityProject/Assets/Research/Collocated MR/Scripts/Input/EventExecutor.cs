using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for pair controller action inputs and event outputs, allowing us to quickly put 
/// together different combinations (e.g. XRInput, OVRInput -> UnityEvent).
/// 
/// See child folders for implementations.
/// </summary>
/// <typeparam name="ActionEventPairType"></typeparam>
/// <typeparam name="ActionType"></typeparam>
/// <typeparam name="EventType"></typeparam>
[Serializable]
public class EventExecutor<ActionEventPairType, ActionType, EventType> : MonoBehaviour
    where ActionType : ExecutorAction
    where EventType : ExecutorEvent
    where ActionEventPairType : ActionEventPair<EventType, ActionType>
{
    public List<ActionEventPairType> ActionEventPairs;

    void Update()
    {
        foreach (ActionEventPairType selector in ActionEventPairs)
        {
            if (selector.listenEvent.ShouldExecute())
            {
                Debug.Log("Executing action for " + selector.listenEvent);
                selector.eventAction.ExecuteAction();
            }
        }
    }
}

[Serializable]
public class ExecutorAction
{
    public virtual void ExecuteAction() { }
}

[Serializable]
public class ExecutorEvent
{
    public bool ExecuteOnOnset = true;
    public bool ExecuteOnContinuing = false;

    [Header("If >0, execute only if we've dwelled on the input for long enough")]
    public float ExecuteOnDwell = 0.0f;

    private float DwellTime;
    public virtual bool ShouldExecute()
    {
        bool activeIgnoringDwell = (ExecuteOnOnset && ActionOnset()) || (ExecuteOnContinuing && ActionContinuing());

        if (ActionOnset() || ActionContinuing())
        {
            DwellTime += Time.deltaTime;
        } else
        {
            DwellTime = 0;
        }

        if (ExecuteOnDwell > 0)
        {
            bool dwellComplete = DwellTime > ExecuteOnDwell;
            if (dwellComplete) DwellTime = 0;
            return dwellComplete;
        } else
        {
            return activeIgnoringDwell;
        }
    }

    public virtual bool ActionOnset()
    {
        return false;
    }

    public virtual bool ActionContinuing()
    {
        return false;
    }
}