using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[Serializable]
public class XRExecutorEvent : ExecutorEvent
{
    public string boolFeatureName;
    public XRNode device;
    private bool lastValue;

    private void TestOutputFeatureNames()
    {
        var devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(device, devices);

        var inputFeatures = new List<UnityEngine.XR.InputFeatureUsage>();
        foreach (InputDevice device in devices)
        {
            // get the specified feature
            device.TryGetFeatureUsages(inputFeatures);
            foreach (InputFeatureUsage feature in inputFeatures)
                Debug.Log("Got controller feature " + feature.name);
        }
    }

    public override bool ActionOnset()
    {
        // TestOutputFeatureNames(); 

        bool active = UsageActive();
        bool started = UsageActive() && !lastValue;
        lastValue = active;
        return started;
    }

    public override bool ActionContinuing()
    {
        bool active = UsageActive();
        lastValue = active;
        return active;
    }

    private bool UsageActive()
    {
        // get the device
        var devices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(device, devices);

        bool triggerValue;
        var inputFeatures = new List<UnityEngine.XR.InputFeatureUsage>();
        foreach (InputDevice device in devices)
        {
            // get the specified feature
            device.TryGetFeatureUsages(inputFeatures);
            InputFeatureUsage feature = inputFeatures.Find(x => x.name.Contains(boolFeatureName) && x.type == typeof(bool));

            // check value of feature
            if (feature != null && device.TryGetFeatureValue(feature.As<bool>(), out triggerValue) && triggerValue) return true;
        }

        return false;
    }

    public override string ToString()
    {
        return "XRExecutorEvent for feature " + boolFeatureName + " XRNode" + device;
    }
}