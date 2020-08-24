using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calibration anchor implementation for a fixed point that the headset will be physically aligned with in reality.
/// </summary>
[Serializable]
public class OnePointAlignment : TransformationToReality
{
    protected override Transformation GetTransformation()
    {
        List<GameObject> trackedObjects = GetTrackedObjects();

        switch (objectMode)
        {
            case TrackedObjectMode.Device:
            case TrackedObjectMode.TrackedObject:
            case TrackedObjectMode.TrackedMarkers:
                {
                    if (trackedObjects.Count != 1)
                    {
                        Debug.LogError("OnePointAlignment - can't align as we have multiple tracked objects or markers to align to, only one allowed.");
                        return null;
                    } else
                    {
                        Debug.Log("Transforming from " + this.transform.position + " to the position for " + trackedObjects[0] + " which is " + trackedObjects[0].transform.position);
                        Transformation transformation = new Transformation();
                        transformation.RotationEuler = new Vector3(0, transform.eulerAngles.y - trackedObjects[0].transform.eulerAngles.y, 0);
                        transformation.RotationQuaternion = Quaternion.Euler(transformation.RotationEuler);

                        // new approach
                        Vector3 objectPosition = trackedObjects[0].transform.position;
                        Vector3 rObjectPosition = objectPosition.RotateAroundPivot(Vector3.zero, transformation.RotationQuaternion);
                        transformation.TranslationVector = (this.transform.position - rObjectPosition);

                        // transformation.TranslationVector = (trackedObjects[0].transform.position - this.transform.position) * -1;

                        Player.GetLocalPlayer().PlayAudioAlert(Resources.Load<AudioClip>("single_point"));

                        return transformation;
                    }
                }
        }
        return null;
    }
}
