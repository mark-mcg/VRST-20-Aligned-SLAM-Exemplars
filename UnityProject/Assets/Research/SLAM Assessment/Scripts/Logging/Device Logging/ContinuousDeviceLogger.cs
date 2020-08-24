using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When enabled it logs the device position every Update.
/// </summary>
public class ContinuousDeviceLogger : DeviceLogger
{
    public bool LogHeadsetDataPeriodically;
    public float sampleRate;
    private float elapsedTime;

    public void Update()
    {
        elapsedTime += Time.deltaTime;

        if (LogHeadsetDataPeriodically)
        {
            if (sampleRate == 0 || elapsedTime >= (1 / sampleRate))
            {
                elapsedTime = 0;
                LogHeadsetData(CurrentPointID, 0);
            }
        }
    }
}
