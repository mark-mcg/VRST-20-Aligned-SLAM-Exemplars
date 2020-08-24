using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LoggingManager : MonoBehaviour
{
    public string FilePrefix;
    public Player deviceToLog;
    public bool AutoSetDeviceToLog = true;

    public static int SESSION_ID = 1337;
    List<BaseLogger> loggersInScene;

    bool LoggingStarted = false;

    public void Start()
    {
        if (AutoSetDeviceToLog)
        {
            if (deviceToLog == null)
                deviceToLog = FindObjectOfType<Player>();

            if (deviceToLog != null)
            {
                foreach (Player dev in FindObjectsOfType<Player>())
                {
                    if (dev != deviceToLog)
                        dev.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ToggleLogging()
    {
        if (!LoggingStarted)
            StartLogging();
        else
            StopLogging();
    }

    public void StartLogging()
    {
        GetActiveLoggers();
        StopLogging();

        SESSION_ID = (int) Random.Range(0, float.MaxValue);
        foreach (BaseLogger logger in loggersInScene)
        {
            logger.StartLogging(FilePrefix);
        }

        LoggingManager.PlayAudioAlert(Resources.Load<AudioClip>("logging_started"));
        LoggingStarted = true;
        Debug.LogError("Logging Started!");
    }

    public void StopLogging()
    {
        GetActiveLoggers();
        foreach (BaseLogger logger in loggersInScene)
        {
            logger.StopLogging();
            logger.CloseLogs();
        }

        LoggingManager.PlayAudioAlert(Resources.Load<AudioClip>("logging_stopped"));
        LoggingStarted = false;
        Debug.LogError("Logging Stopped!");

    }

    void GetActiveLoggers()
    {
        loggersInScene = FindObjectsOfType<BaseLogger>().ToList();    
    }

    public static void PlayAudioAlert(AudioClip alert)
    {
        LoggingManager manager = FindObjectOfType<LoggingManager>();
        Debug.Log("Got manager " + manager );
        Debug.Log("... with devicetolog of " + manager.deviceToLog);

        Player player = manager.deviceToLog.GetComponent<Player>();
        player.PlayAudioAlert(alert);
    }
}
