using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Records device log data periodically based on event triggers. Can either 
/// 
/// * Record LogXFramesOnEvent frames in a row based on a call to RecordHeadsetDataForFrames
/// * Record LogXFramesOnEvent when the event finishes, counting how many seconds the event was active for
/// to indicate the ID of the point being recorded in reality.
/// 
/// </summary>
public class PeriodicDeviceLogger : DeviceLogger
{
    public int LogXFramesOnEvent = 5;
    private int logForFrames = 0;
    private float onsetLogEventTime = -1;
    private float lastLogEventTime = -1;
    private int lastLogEventTick;

    /// <summary>
    /// For UI button presses where a button is held and events are triggered at pointer down/up
    /// </summary>
    private bool discreteEventStart = false;
    public void RecordEventStart()
    {
        discreteEventStart = true;
    }

    public void RecordEventStop()
    {
        discreteEventStart = false;
    }

    /// <summary>
    /// For XR controller presses where the button is held and an event triggered each frame
    /// </summary>
    public void RecordHeadsetDataWhenInputCeases()
    {
        if (onsetLogEventTime == -1)
            onsetLogEventTime = Time.time;
        lastLogEventTime = Time.time;
    }

    /// <summary>
    /// For instantaneously logging the points
    /// </summary>
    /// <param name="id"></param>
    public void RecordHeadsetDataForFrames(int id)
    {
        CurrentPointID = id;
        logForFrames = LogXFramesOnEvent;
    }

    public void Update()
    {
        if (discreteEventStart)
            RecordHeadsetDataWhenInputCeases();

        if (lastLogEventTime != -1)
        {
            int currentLogEventTick = Mathf.FloorToInt(Time.time - onsetLogEventTime);

            if (currentLogEventTick > lastLogEventTick)
            {
                LoggingManager.PlayAudioAlert(Resources.Load<AudioClip>("tick"));
                lastLogEventTick = currentLogEventTick;
            }

            // if there's a delay between ticks, assume user has stopped input and log event
            if ((Time.time - lastLogEventTime) >= 0.1)
            {
                onsetLogEventTime = -1;
                lastLogEventTime = -1;

                logForFrames = LogXFramesOnEvent;
                CurrentPointID = currentLogEventTick;
                lastLogEventTick = 0;

                Debug.Log("PerdiodicDeviceLogger recording for ID " + CurrentPointID);
            }
        }

        if (logForFrames > 0)
        {
            logForFrames--;
            LogHeadsetData(CurrentPointID, logForFrames);

            if (logForFrames == 0)
            {
                LoggingManager.PlayAudioAlert(Resources.Load<AudioClip>("point_recorded"));
                currentBlock++;
            }
        }
    }
}