using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResultLogger : MonoBehaviour
{
    public abstract void LogResult(Loggable result);
    public abstract void CloseLogs();
}


/// <summary>
/// Basic result logger component - writes each result as a row in a JSON List, with results optionally split up
/// based on type.
/// </summary>
public class JSONResultLogger : ResultLogger
{
    [Header("JSONResultLogger")]
    public string folder;
    public string filedescriptor;
    public bool seperateFileForDifferentResultTypes = false;
    public bool loggingEnabled = false;

    private Dictionary<System.Type, DynamicJsonList> resultLists = new Dictionary<System.Type, DynamicJsonList>();

    /// <summary>
    /// Record the result to a JSON file, creating the file if none exists based on the folder and filedescriptor paramaters.
    /// </summary>
    /// <param name="result"></param>
    public override void LogResult(Loggable result)
    {
        if (loggingEnabled)
        {
            DynamicJsonList logList;
            System.Type key = seperateFileForDifferentResultTypes ? result.GetType() : typeof(Loggable);

            if (!resultLists.ContainsKey(result.GetType()))
            {
                resultLists.Add(result.GetType(), new DynamicJsonList(folder, GetLogFilePrefix() + "_" + result.GetType()));
            }

            logList = resultLists[result.GetType()];
            logList.RecordRow(result);
        }
    }

    public virtual string GetLogFilePrefix()
    {
        return filedescriptor;
    }

    public override void CloseLogs()
    {
        Debug.Log("CloseLogs called");
        foreach (DynamicJsonList list in resultLists.Values)
        {
            list.CloseList();
        }
        resultLists.Clear();
    }
}