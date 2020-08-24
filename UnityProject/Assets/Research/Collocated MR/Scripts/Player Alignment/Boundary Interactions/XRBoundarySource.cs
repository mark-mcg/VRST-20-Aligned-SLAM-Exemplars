using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRBoundarySource : BoundarySource
{
    public override List<Vector3> GetBoundary()
    {
        List<Vector3> boundary = new List<Vector3>();
        List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances<XRInputSubsystem>(inputSubsystems);
        if (inputSubsystems.Count > 0)
        {
            Debug.Log("Got input subsystem");
            if (inputSubsystems[0].TryGetBoundaryPoints(boundary))
            {
                Debug.Log("Got boundary of length " + boundary.Count);
            }
        }

        return boundary;
    }
}