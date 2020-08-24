using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Aligns this game object to a Player UIPosition when one is found, if it is of the specified device type
/// </summary>
public class AlignToUIPosition : MonoBehaviour
{
    public Canvas canvas;
    UIPosition uiPosition;
    public DevicePrefabMap.XRDeviceType SpecificDeviceTypesOnly;
    private float lastCheck;
    public bool enableOnAttach = true;

    void Update()
    {
        if (Time.time - lastCheck > 0.5f)
        {
            lastCheck = Time.time;

            if (Player.GetLocalPlayer() != null)
            {
                // got a local player, now try assigning UI
                uiPosition = Player.GetLocalPlayer().GetComponentInChildren<UIPosition>();

                if (uiPosition != null)
                {
                    if (SpecificDeviceTypesOnly == null ||
                        (SpecificDeviceTypesOnly != null && Player.GetLocalPlayer() != null &&
                        Player.GetLocalPlayer().deviceType.HasOneOrMoreFlags(SpecificDeviceTypesOnly.flags)))
                    {
                        canvas.gameObject.SetActive(enableOnAttach);
                        transform.SetParent(uiPosition.transform);
                        this.transform.localPosition = Vector3.zero;
                        this.transform.localEulerAngles = Vector3.zero;

                        RectTransform rectTransform = GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.localPosition = Vector3.zero;
                            rectTransform.localEulerAngles = Vector3.zero;
                        }

                        // events won't work correctly without main camera set
                        canvas.GetComponentInChildren<Canvas>().worldCamera = Player.GetLocalPlayer().mainCamera;
                    }
                }
                enabled = false;
            }
        }
    }
}
