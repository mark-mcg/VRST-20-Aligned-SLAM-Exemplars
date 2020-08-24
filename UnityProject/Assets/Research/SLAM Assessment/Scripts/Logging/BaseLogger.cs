using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseLogger : JSONResultLogger
{
    protected LoggingManager loggingManager;

    public virtual void Awake()
    {
        loggingManager = FindObjectOfType<LoggingManager>();
    }

    private string Prefix = "";
    public virtual void StartLogging(string prefix = "")
    {
        Prefix = prefix;
        loggingEnabled = true;
    }

    public override string GetLogFilePrefix()
    {
        if (Prefix != null && Prefix.Length > 0)
            return base.GetLogFilePrefix() + "_" + Prefix;
        else
            return base.GetLogFilePrefix();
    }

    public virtual void StopLogging()
    {
        loggingEnabled = false;
        Prefix = "";
    }
}