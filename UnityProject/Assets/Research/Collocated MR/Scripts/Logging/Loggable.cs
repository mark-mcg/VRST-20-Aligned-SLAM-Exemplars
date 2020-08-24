using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoggable : ILoggableProvider
{
    void StartLogging();
    void StopLogging();
}

public interface ILoggableProvider
{
    Loggable GetLoggable();
}

[JsonObject(MemberSerialization.Fields)]
public class Loggable {

    public float timeSinceStartObjectCreated;
    public long epochTimeObjectCreated;
    public long currentRenderFrame;

    public Loggable()
    {
        timeSinceStartObjectCreated = Time.time;
        epochTimeObjectCreated = System.DateTime.Now.Ticks;
        currentRenderFrame = Time.frameCount;
    }

    /// <summary>
    /// If there's any last calculation to be performed prior to logging, do it here
    /// </summary>
    public virtual void FinalizeResult(){}
}
