using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TransformationToReality : MonoBehaviour
{
    /// <summary>
    /// TODO - change all the alignment classes to return the same result and apply it?
    /// </summary>
    public class Transformation
    {
        public Vector3 RotationEuler, TranslationVector;
        public Quaternion RotationQuaternion;

        public override string ToString()
        {
            return "Translation " + TranslationVector + " Rotation " + RotationEuler;
        }
    }

    public delegate void TransformationEvent();
    public event TransformationEvent OnTransformationApplied;

    public enum TrackedObjectMode {
        Device,          // This will use the main camera on the Player object, assuming this is the headset
        TrackedObject,    // This will use the object assigned to TrackedObject (e.g. controller ) whose position is known relative to the headset
        TrackedMarkers    
    }

    [Header("TransformationToReality Settings")]
    public TrackedObjectMode objectMode = TrackedObjectMode.Device;
    
    [Header("For TrackedObject / TrackedMarkers modes only")]
    public List<GameObject> TrackedObjectsOrMarkers;

    /// <summary>
    /// Ignore the height if you are going to be placing the headset at a known point in reality in X/Z, not accounting for height in Y 
    /// (i.e. you want to preserve the actual height of the headset from the floor for a roomscale shared VR experience)
    /// 
    /// Don't ignore the height if your known point is where you expect their head to roughly be (e.g. an audience member in a seat for a seated VR experience).
    /// </summary>
    [Header("Don't correct the Y position of the device (e.g. a headset where it knows its height from the floor and you are aligning to a point on x/z)")]
    public bool IgnoreYPosition = true;

    #region Methods to overload
    protected abstract Transformation GetTransformation();
    #endregion

    public bool TryEnactTransformation()
    {
        Debug.Log("TryEnactTransformation for " + gameObject.name);

        // Note - any existing transformation will be reset!
        ZeroPlayerOffset();
        tGainWasEnabled = SetTranslationalGain(false);
        Transformation result = GetTransformation();
        SetTranslationalGain(tGainWasEnabled);

        if (result != null)
        {
            // Note the current camera world position
            // Vector3 cameraPosition = TrackedObjectsOrMarkers[0].transform.position;

            // Rotate our parent object
            Player.GetLocalPlayer().RigSceneAnchorOffset.transform.rotation = result.RotationQuaternion;

            // Rotating the parent means the camera has changed orientation and position (as it was offset from our parent)
            // so we need to undo the translation the camera has experienced
            //Vector3 cameraMovement = -1 * (TrackedObjectsOrMarkers[0].transform.position - cameraPosition);

            // Apply our translation to align to reality, and undo the camera movement caused by the rotation
            Player.GetLocalPlayer().RigSceneAnchorOffset.transform.localPosition = // cameraMovement +
                new Vector3(result.TranslationVector.x, 
                            IgnoreYPosition ? Player.GetLocalPlayer().RigSceneAnchorOffset.transform.localPosition.y : result.TranslationVector.y, 
                            result.TranslationVector.z);

            //switch (objectMode)
            //{
            //    case TrackedObjectMode.Device:
            //        {
            //            // Note the current camera world position
            //            Vector3 cameraPosition = Player.GetLocalPlayer().mainCamera.transform.position;

            //            // Rotate our parent object
            //            Player.GetLocalPlayer().RigSceneAnchorOffset.transform.rotation = result.RotationQuaternion;

            //            // Rotating the parent means the camera has changed orientation and position (as it was offset from our parent)
            //            // so we need to undo the translation the camera has experienced
            //            Vector3 cameraMovement = -1 * (Player.GetLocalPlayer().mainCamera.transform.position - cameraPosition);

            //            // Apply our translation to align to reality, and undo the camera movement caused by the rotation
            //            Player.GetLocalPlayer().RigSceneAnchorOffset.transform.localPosition += cameraMovement +
            //                new Vector3(result.TranslationVector.x, IgnoreYPosition ? 0 : result.TranslationVector.y, result.TranslationVector.z);
            //            break;
            //        }
            //    case TrackedObjectMode.TrackedObject:
            //        {
            //            // Need to transform headset/device to tracked object, then from tracked object to real world
            //            if (TrackedObjectsOrMarkers.Count > 0)
            //            {
            //                Transformation TrackedObjectToHeadset = new Transformation()
            //                {
            //                    TranslationVector = TrackedObjectsOrMarkers[0].transform.position - Player.GetLocalPlayer().mainCamera.transform.position,
            //                    RotationQuaternion = Quaternion.FromToRotation(TrackedObjectsOrMarkers[0].transform.forward, Player.GetLocalPlayer().transform.forward)
            //                };

            //                Player.GetLocalPlayer().RigSceneAnchorOffset.transform.RotateAroundPivot(Player.GetLocalPlayer().RigSceneAnchorOffset.transform.position, TrackedObjectToHeadset.RotationQuaternion * result.RotationQuaternion);

            //                Player.GetLocalPlayer().RigSceneAnchorOffset.transform.localPosition =
            //                    new Vector3(result.TranslationVector.x, IgnoreYPosition ? 0 : result.TranslationVector.y, result.TranslationVector.z);
            //            } else
            //            {
            //                Debug.LogError("No Tracked Object to create transformation from/to");
            //            }
            //            break;
            //        }
            //}

            OnTransformationApplied?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// If Player is already aligned to a point, undo this alignment first.
    /// </summary>
    private void ZeroPlayerOffset()
    {
        Player local = Player.GetLocalPlayer();
        // retain y position e.g. for devices where this is set separetely to indicate floor level
        local.RigSceneAnchorOffset.transform.localPosition = new Vector3(0, local.RigSceneAnchorOffset.transform.localPosition.y, 0);
        local.RigSceneAnchorOffset.transform.localRotation = Quaternion.identity;
    }

    protected List<GameObject> GetTrackedObjects()
    {
        List<GameObject> result = new List<GameObject>();
        switch (objectMode)
        {
            case TrackedObjectMode.Device:
                result.Add(Player.GetLocalPlayer().mainCamera.gameObject);
                break;
            case TrackedObjectMode.TrackedObject:
                result.Add(TrackedObjectsOrMarkers[0]);
                break;
            case TrackedObjectMode.TrackedMarkers:
                result.AddRange(TrackedObjectsOrMarkers);
                break;
        }
        return result;
    }

    private bool tGainWasEnabled = false;
    /// <summary>
    /// When doing alignments, we need to make sure TranslationalGain is disabled, so we are getting the
    /// true position of the MR device in the real world.
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    private bool SetTranslationalGain(bool enabled)
    {
        TranslationalGain tGain = Player.GetLocalPlayer().GetComponentInChildren<TranslationalGain>();
        bool wasEnabled = false;

        if (tGain != null)
        {
            wasEnabled = tGain.enabled;
            tGain.enabled = enabled;
        }

        return wasEnabled;
    }
}
