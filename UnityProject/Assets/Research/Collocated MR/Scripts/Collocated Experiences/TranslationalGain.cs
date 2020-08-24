using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/* 
 * Author: Mark McGill
 * Edited from ProportionalInverseMove.cs from my own project. If it's broken let me know!
 *
 * Enables translational gain based on a tracked object. Does so by moving the SteamVR playarea.
 * In this way, the safety boundaries relative to the real physical area are preserved.
 *
 * N.b. There is also now a VRTK add on for applying translational gain which might be more general purpose use.
 * See: https://vrtoolkit.readme.io/docs/vrtk_roomextender
 */
public class TranslationalGain : MonoBehaviour
{

    [Header("Object whose position should be tracked for the accelerated movement (e.g. HMD, controller, tracker...)")]
    public GameObject trackedObjectInPlayArea;

    [Header("The play area to be proportionally moved.")]
    public GameObject trackingArea;

    [Header("Gain to be applied (0=1.0x/no gain, 1=2.0x gain)")]
    public float ratio = 0.2f;

    [Header("Invert direction of movement")]
    public bool invert = false;

    [Header("Objects that should be scaled in position/size to match gain (e.g. obstacles)")]
    public List<SceneObject> managedSceneObjects = new List<SceneObject>();

    private Vector3 playAreaCenter;

    public void Start()
    {
        if (trackingArea == null)
            trackingArea = this.gameObject;
        playAreaCenter = trackingArea.transform.position;
        setRatio(ratio);
    }

    void Update()
    {
        if (trackedObjectInPlayArea != null && trackingArea != null)
            movePlayArea();
    }

    public void OnDisable()
    {
        trackingArea.transform.localPosition = Vector3.zero;
    }

    public void setRatio(float newRatio)
    {
        ratio = newRatio;

        foreach (SceneObject so in managedSceneObjects)
        {
            so.setRatio(ratio);
        }
    }

    [Serializable]
    public class SceneObject
    {
        public bool shouldRearrange = true;
        public bool shouldRescale = true;
        public GameObject managedObject;

        private Vector3 originalPosition;
        private Vector3 originalScale;
        private bool isSetup = false;

        TranslationalGain pimove;

        public SceneObject(GameObject go, bool rearrange = true, bool rescale = true)
        {
            managedObject = go;
            shouldRearrange = true;
            shouldRescale = true;

            setup();
        }


        private void setup()
        {
            originalPosition = managedObject.transform.position;
            originalScale = managedObject.transform.localScale;
            pimove = GameObject.FindObjectOfType<TranslationalGain>();
            isSetup = true;
        }

        public void setRatio(float ratio)
        {
            float multiplier = 1 + ratio;

            if (!isSetup)
                setup();

            if (shouldRescale)
            {
                Vector3 newScale = new Vector3(originalScale.x * multiplier, originalScale.y, originalScale.z * multiplier);
                managedObject.transform.localScale = newScale;
            }

            if (shouldRearrange)
            {
                Vector3 newPosition = pimove.getWorldPositionBasedOnRatio(originalPosition);
                managedObject.transform.position = newPosition;
            }
        }
    }

    public void addObjectToManage(GameObject go)
    {
        SceneObject so = new SceneObject(go);
        managedSceneObjects.Add(so);
        so.setRatio(ratio);
    }

    public void removeObjectToManage(GameObject go)
    {
        SceneObject item = managedSceneObjects.FirstOrDefault(x => x.managedObject == go);
        if (item != null)
        {
            managedSceneObjects.Remove(item);
            item.setRatio(1.0f);
        }
    }

    private Vector3 getPositionBasedOnRatio(Vector3 originalPosition, Vector3 offsetToPlayArea, bool ignoreY = true, bool invert = false)
    {
        Vector3 offset = offsetToPlayArea * ratio * (invert ? -1 : 1);

        Vector3 newPosition = originalPosition + offset;

        newPosition = new Vector3(newPosition.x, ignoreY ? originalPosition.y : newPosition.y, newPosition.z);

        return newPosition;
    }

    public Vector3 getWorldPositionBasedOnRatio(Vector3 originalPosition, bool ignoreY = true)
    {
        return getPositionBasedOnRatio(originalPosition, (originalPosition - playAreaCenter));
    }

    public void movePlayArea(bool ignoreY = true)
    {
        Vector3 newPosition = getPositionBasedOnRatio(playAreaCenter, trackedObjectInPlayArea.transform.localPosition, true, false);
        trackingArea.transform.localPosition = newPosition;
    }
}