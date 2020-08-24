using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DevicePrefabMap : MonoBehaviour
{
    [Serializable]
    public class XRDeviceType
    {
        [Flags]
        public enum XRDeviceTypes {
            XR = (1 << 0 ),
            Oculus = (1 << 1),
            ARCore = (1 << 2)
        }

        public XRDeviceTypes flags;

        public int GetMatchingFlags(XRDeviceTypes toMatch)
        {
            int matchedFlags = 0;
            foreach (XRDeviceTypes devType in Enum.GetValues(typeof(XRDeviceTypes)))
            {
                // Debug.Log("For flag " + devType + " we have flag " + type.HasFlag(devType) + " they have flag " + ((XRDeviceType)obj).type.HasFlag(devType));
                if (this.flags.HasFlag(devType))
                {
                    matchedFlags += toMatch.HasFlag(devType) ? 1 : 0;
                }
            }
            return matchedFlags;
        }

        public bool HasOneOrMoreFlags(XRDeviceTypes toMatch)
        {
            return GetMatchingFlags(toMatch) > 0;
        }

        public bool HasFlag(XRDeviceTypes toMatch)
        {
            return flags.HasFlag(toMatch);
        }

        public bool HasAllFlags(XRDeviceTypes toMatch)
        {
            bool hasAllFlags = true;
            foreach (XRDeviceTypes devType in Enum.GetValues(typeof(XRDeviceTypes)))
            {
                // Debug.Log("For flag " + devType + " we have flag " + type.HasFlag(devType) + " they have flag " + ((XRDeviceType)obj).type.HasFlag(devType));
                if (flags.HasFlag(devType))
                {
                    hasAllFlags &= toMatch.HasFlag(devType);
                }
            }
            return hasAllFlags;
        }

        public override bool Equals(object obj)
        {
            if (obj is XRDeviceType)
            {
                return HasAllFlags(((XRDeviceType)obj).flags);
            }

            return base.Equals(obj);
        }

        public override string ToString()
        {
            return String.Join(",", Enum.GetValues(typeof(XRDeviceTypes)).OfType<XRDeviceTypes>().ToList().Where(x => flags.HasFlag(x)));
        }

        public XRDeviceType() { }

        public XRDeviceType(XRDeviceTypes type)
        {
            this.flags = type;
        }
    }

    public static XRDeviceType TYPE_OCULUS = new XRDeviceType(XRDeviceType.XRDeviceTypes.Oculus);
    public static XRDeviceType TYPE_GENERIC_XR = new XRDeviceType(XRDeviceType.XRDeviceTypes.XR);
    public static XRDeviceType TYPE_ARCORE = new XRDeviceType(XRDeviceType.XRDeviceTypes.ARCore);

    [Serializable]
    public class PlayerPrefabType
    {
        public bool enabled = true;
        public XRDeviceType deviceType;
        public GameObject prefab;
    }

    [Header("Pre-registered Player Prefab Types (overrides default player prefab)")]
    public List<PlayerPrefabType> playerPrefabTypes = new List<PlayerPrefabType>();
    public XRDeviceType.XRDeviceTypes DefaultType = XRDeviceType.XRDeviceTypes.XR;

    public XRDeviceType GetThisDeviceType()
    {
        XRDeviceType.XRDeviceTypes type = DefaultType;

        if (Application.platform == RuntimePlatform.Android)
        {
            if (SystemInfo.deviceModel.ToLower().Contains("oculus"))
            {
                type |= XRDeviceType.XRDeviceTypes.Oculus;
            }
            else
            {
                type = XRDeviceType.XRDeviceTypes.ARCore;
            }
        }

        return new XRDeviceType(type);
    }

    public PlayerPrefabType GetPrefabType(XRDeviceType deviceType)
    {
        PlayerPrefabType deviceTypePrefab = null;

        Debug.Log("Finding prefab for type " + deviceType);

        if (playerPrefabTypes.Count > 0) {
            foreach (XRDeviceType.XRDeviceTypes devType in Enum.GetValues(typeof(XRDeviceType.XRDeviceTypes)))
            {
                if (deviceType.flags.HasFlag(devType)){
                    PlayerPrefabType prefab = playerPrefabTypes.Single(x => x.deviceType.flags.HasFlag(devType));
                    if (prefab != null && prefab.enabled)
                        deviceTypePrefab = prefab;
                }
            }
        }

        return deviceTypePrefab;
    }
}
