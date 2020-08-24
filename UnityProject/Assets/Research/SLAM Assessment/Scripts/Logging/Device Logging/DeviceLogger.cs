using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceLogger : BaseLogger
{
    [Header("HeadsetResultLogger")]
    public Player deviceToLog;
    public bool PlaceIndicator = false;

    public int CurrentPointID;
    protected int currentBlock;

    private AlignedPointManager alignedPointManager;

    public override void Awake()
    {
        base.Awake();
        alignedPointManager = FindObjectOfType<AlignedPointManager>();
    }

    public Player GetDeviceToLog()
    {
        if (deviceToLog == null)
            return loggingManager.deviceToLog;
        else
            return deviceToLog;
    }

    public void LogHeadsetData(int id, int frameCount)
    {
        if (PlaceIndicator)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.transform.position = Player.GetLocalPlayerCamera().transform.position;
            indicator.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        }

        DeviceLoggable loggable = new DeviceLoggable(loggingManager.deviceToLog.gameObject.name, 
            loggingManager.deviceToLog.mainCamera.transform, 
            alignedPointManager.CurrentAnchor,
            frameCount, 
            currentBlock, 
            id);
        LogResult(loggable);
    }

    public void OnApplicationQuit()
    {
        CloseLogs();
    }
}

public class DeviceLoggable : Loggable
{
    public string DeviceName;
    public Vector3 WorldPosition;
    public Vector3 LocalPosition;
    public Vector3 WorldEuler;
    public Vector3 LocalEuler;

    public Vector3 WorldPositionDistanceFromAnchor;
    public float EuclideanPositionDistanceFromAnchor;
    public Vector3 WorldEulerDistanceFromAnchor;
    public float MinAngleFromAnchor;
    public Vector3 AnchorPosition;
    public Vector3 AnchorEuler;
    public int frame;
    public int block;
    public int session;
    public int frameCount;
    public int pointID;

    public DeviceLoggable(string deviceName, Transform positionIndicator, TransformationToReality anchor, int framecount, int block, int pointID)
    {
        DeviceName = deviceName;
        WorldPosition = positionIndicator.position;
        LocalPosition = positionIndicator.localPosition;

        WorldEuler = positionIndicator.eulerAngles;
        LocalEuler = positionIndicator.localEulerAngles;
        //if (deviceName.Contains("ZED") || deviceName.Contains("Camera_eyes"))
        //    LocalEulerRelativeToStart = new Vector3(LocalEulerRelativeToStart.z, LocalEulerRelativeToStart.y, LocalEulerRelativeToStart.x);
        this.frame = framecount;
        this.session = LoggingManager.SESSION_ID;
        this.block = block;
        frameCount = Time.frameCount;
        this.pointID = pointID;


        if (anchor != null)
        {
            WorldPositionDistanceFromAnchor = anchor.transform.position - positionIndicator.position;
            EuclideanPositionDistanceFromAnchor = Vector3.Distance(anchor.transform.position, positionIndicator.position);
            WorldEulerDistanceFromAnchor = anchor.transform.eulerAngles - positionIndicator.eulerAngles;
            MinAngleFromAnchor = Vector3.Angle(anchor.transform.forward, positionIndicator.forward);
            AnchorPosition = anchor.transform.position;
            AnchorEuler = anchor.transform.eulerAngles;
        }
    }
}
