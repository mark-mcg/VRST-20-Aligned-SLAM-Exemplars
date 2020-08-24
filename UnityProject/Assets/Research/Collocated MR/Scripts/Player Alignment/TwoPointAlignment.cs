using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implementation based on DeFanti's work, see https://pdfs.semanticscholar.org/739f/f90b2805ead8199f35a938328620521ffdd2.pdf 
/// and https://frl.nyu.edu/a-quick-and-easy-calibration-method/
/// </summary>
public class TwoPointAlignment : TransformationToReality
{
    [Header("Two Point Alignment Settings")]
    public GameObject VirtualPoint1, VirtualPoint2;
    protected Vector3 RecordedPoint1, RecordedPoint2;
    private Transformation LastTransformation;
    private int CurrentPointOfNote = 1;

    protected override Transformation GetTransformation()
    {
        List<GameObject> trackedObjects = GetTrackedObjects();

        switch (objectMode)
        {
            case TrackedObjectMode.Device:
            case TrackedObjectMode.TrackedObject:
                {
                    // if we are tracking a headset or controller
                    // then assume we are enacting the two point selections sequentially to create a transformation
                    if (trackedObjects.Count == 1)
                    {
                        if (CurrentPointOfNote == 1)
                        {
                            LastTransformation = null;
                            CheckPoint(1, trackedObjects[0]);
                            Player.GetLocalPlayer().PlayAudioAlert(Resources.Load<AudioClip>("two_point_1"));
                            CurrentPointOfNote = 2;
                            return null;
                        }
                        else
                        {
                            CheckPoint(2, trackedObjects[0]);
                            CurrentPointOfNote = 1;
                            Player.GetLocalPlayer().PlayAudioAlert(Resources.Load<AudioClip>("two_point_2"));
                            LastTransformation = PerformTwoPointAlignment();
                            return LastTransformation;
                        }
                    }
                    break;
                }
            case TrackedObjectMode.TrackedMarkers:
                {
                    // if we are tracking two QR codes or other markers in the environment
                    // then we will check both markers - if active, we note their position,
                    // when we have both positions we then generate an alignment
                    if (trackedObjects.Count == 2)
                    {
                        CheckPoint(1, trackedObjects[0]);
                        CheckPoint(2, trackedObjects[1]);

                        if (RecordedPoint1 != Vector3.zero && RecordedPoint2 != Vector3.zero)
                        {
                            Player.GetLocalPlayer().PlayAudioAlert(Resources.Load<AudioClip>("two_point_both"));
                            LastTransformation = PerformTwoPointAlignment();
                            return LastTransformation;
                        }
                    }
                    break;
                }
        }

        return null;
    }

    private bool CheckPoint(int point, GameObject trackedObject)
    {
        if (trackedObject != null && trackedObject.activeInHierarchy)
        {
            if (point == 1)
            {
                RecordedPoint1 = trackedObject.transform.position;
            }
            else
            {
                RecordedPoint2 = trackedObject.transform.position;
            }
            return true;
        }
        return false;
    }

    public Transformation PerformTwoPointAlignment()
    {
        return PerformTwoPointAlignment(VirtualPoint1, VirtualPoint2, RecordedPoint1, RecordedPoint2);
    }

    public Transformation PerformTwoPointAlignment(GameObject VirtualPoint1, GameObject VirtualPoint2, Vector3 RecordedPoint1, Vector3 RecordedPoint2)
    {
        return PerformTwoPointAlignment(VirtualPoint1.transform.position, VirtualPoint2.transform.position, RecordedPoint1, RecordedPoint2);
    }

    public Transformation PerformTwoPointAlignment(Vector3 P1Position, Vector3 P2Position, Vector3 Q1Position, Vector3 Q2Position)
    {
        Transformation result = new Transformation();

        // Zero the heights for all points (only align on X and Z, not Y - most devices report Y relative to floor)
        P1Position = new Vector3(P1Position.x, 0, P1Position.z);
        P2Position = new Vector3(P2Position.x, 0, P2Position.z);
        Q1Position = new Vector3(Q1Position.x, 0, Q1Position.z);
        Q1Position = new Vector3(Q1Position.x, 0, Q1Position.z);

        // Calculate the rotation
        Vector3 vectorBetweenQ12 = Q2Position - Q1Position;
        Vector3 vectorBetweenP12 = P2Position - P1Position;
        float QBearing = Quaternion.LookRotation(vectorBetweenQ12, Vector3.up).eulerAngles.y;
        float PBearing = Quaternion.LookRotation(vectorBetweenP12, Vector3.up).eulerAngles.y;
        float QPBearingDifference = PBearing - QBearing;
        result.RotationEuler = new Vector3(0, QPBearingDifference, 0);
        result.RotationQuaternion = Quaternion.Euler(result.RotationEuler);

        // Then calculate the translation
        // First, rotate the recorded device points Q around origin, aligning them with the bearing of the points P
        Vector3 Q1PositionR = Q1Position.RotateAroundPivot(Vector3.zero, result.RotationQuaternion);
        Vector3 Q2PositionR = Q2Position.RotateAroundPivot(Vector3.zero, result.RotationQuaternion);

        // Then find the difference in position between QR and P
        result.TranslationVector = ((P1Position + P2Position) / 2) - ((Q1PositionR + Q2PositionR) / 2);

        return result;
    }

    GameObject anchor;
    protected void TestTwoPointAlignment()
    {
        anchor = new GameObject("Anchor");

        float distanceBetweenPoints = 10f;
        GameObject P1, P2, Q1, Q2;

        P1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        P1.name = "P1";
        P1.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0); // rotation only matters insofar as placing the second point roughly the expected distance between the points
        P1.transform.position = new Vector3(Random.Range(0, 5), 0, Random.Range(0, 5));

        P2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        P2.name = "P2";
        P2.transform.position = P1.transform.position + (P1.transform.forward * distanceBetweenPoints);
        P2.transform.forward = P1.transform.forward;

        Q1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Q1.name = "Q1";
        Q1.transform.SetParent(anchor.transform);
        Q1.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0); // rotation only matters insofar as placing the second point roughly the expected distance between the points
        Q1.transform.position = new Vector3(Random.Range(0, 5), 0, Random.Range(0, 5));

        Q2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Q2.name = "Q2";
        Q2.transform.SetParent(anchor.transform);
        Q2.transform.position = Q1.transform.position + (Q1.transform.forward * distanceBetweenPoints);
        Q2.transform.forward = Q1.transform.forward;

        Transformation result = PerformTwoPointAlignment(P1, P2, Q1.transform.position, Q2.transform.position);
        Debug.Log(result);
        gameObject.transform.rotation = result.RotationQuaternion;
        gameObject.transform.localPosition = result.TranslationVector;
    }
}

/// <summary>
/// From https://answers.unity.com/questions/47115/vector3-rotate-around.html
/// </summary>
public static class RotateAroundPivotExtensions
{
    //Returns the rotated Vector3 using a Quaterion
    public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Quaternion Angle)
    {
        return Angle * (Point - Pivot) + Pivot;
    }
    //Returns the rotated Vector3 using Euler
    public static Vector3 RotateAroundPivot(this Vector3 Point, Vector3 Pivot, Vector3 Euler)
    {
        return RotateAroundPivot(Point, Pivot, Quaternion.Euler(Euler));
    }
    //Rotates the Transform's position using a Quaterion
    public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Quaternion Angle)
    {
        Me.position = Me.position.RotateAroundPivot(Pivot, Angle);
    }
    //Rotates the Transform's position using Euler
    public static void RotateAroundPivot(this Transform Me, Vector3 Pivot, Vector3 Euler)
    {
        Me.position = Me.position.RotateAroundPivot(Pivot, Quaternion.Euler(Euler));
    }
}
