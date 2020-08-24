using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

/// <summary>
/// Monitors the Player camera (headset) local position, and reports on possible large changes
/// of position that defy what is physically possible through movement alone 
/// i.e. the tracking has re-oriented the player elsewhere, and our calibration may now
/// be invalid.
/// </summary>
public class TrackingFailureDetector : MonoBehaviour
{
    [HideInInspector]
    public bool UserPresent;
    public Transform UserHead;
    public float DistanceThreshold = 0.2f;

    [HideInInspector]
    public Vector3 lastLocalPosition;

    public UnityEvent ThresholdExceeded;


    void Update()
    {
        CheckUserPresence();

        if (lastLocalPosition == Vector3.zero)
            lastLocalPosition = UserHead.localPosition;

        float distance = Vector3.Distance(UserHead.localPosition, lastLocalPosition);
        if (UserPresent && distance >= DistanceThreshold)
        {
            Debug.LogError("TrackingFailureDetector: Distance threshold exeeded, distance of " + distance);
            ThresholdExceeded?.Invoke();
        }

        lastLocalPosition = UserHead.localPosition;
    }


    void CheckUserPresence()
    {
        var devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeadMounted, devices);
        bool userPresence = true; // default to true for devices that aren't XR compatible
        
        if (devices != null && devices.Count > 0)
            userPresence = devices[0].TryGetFeatureValue(CommonUsages.userPresence, out userPresence) && userPresence;
        
        UserPresent = userPresence;
    }
}
