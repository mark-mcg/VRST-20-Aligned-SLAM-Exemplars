using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnityEventExecutorAction : ExecutorAction
{
    public UnityEvent unityEvent;

    public override void ExecuteAction()
    {
        base.ExecuteAction();
        unityEvent?.Invoke();
    }
}

