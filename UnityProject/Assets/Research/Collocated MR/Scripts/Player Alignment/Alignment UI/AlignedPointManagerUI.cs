using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Generates buttons in the player UI for all alignment points added to the alignedpointmanager. This could be useful if
/// you have multiple alignment points in reality/virtuality e.g. one for each seat in a seated experience, or one for each
/// standing position at the start of the experience. So long as the user knows which point they are at (and is in the correct
/// orientation) they can then select that point in this menu and calibrate to it.
/// </summary>
public class AlignedPointManagerUI : MonoBehaviour
{
    AlignedPointManager manager;
    public GameObject contentGroup;
    public UnityEvent OnAlignment;

    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<AlignedPointManager>();

        int ind = 0;
        foreach (TransformationToReality anchor in manager.Anchors.OrderBy(x => x.gameObject.name))
        {
            GameObject buttonPrefab = Instantiate(Resources.Load("AlignmentPointButton")) as GameObject;
            buttonPrefab.transform.SetParent(contentGroup.transform);
            buttonPrefab.transform.SetSiblingIndex(ind);
            ind++;
            buttonPrefab.name = "Align to: " + anchor.gameObject.name;
            buttonPrefab.GetComponentInChildren<TextMeshProUGUI>().text = anchor.gameObject.name; // + " ("+ anchor.GetType() + ")";
            buttonPrefab.GetComponentInChildren<Button>().onClick.AddListener(delegate {
                manager.AlignTo(anchor);
                SetUIVisibility(false);
                OnAlignment?.Invoke();
            });
            buttonPrefab.GetComponentInChildren<RectTransform>().localScale = Vector3.one;
            buttonPrefab.GetComponentInChildren<RectTransform>().localPosition= Vector3.zero;
            buttonPrefab.GetComponentInChildren<RectTransform>().localEulerAngles = Vector3.zero;

        }
    }

    public void SetUIVisibility(bool visibility)
    {
        UIPosition position = Player.GetLocalPlayer()?.GetComponentInChildren<UIPosition>();

        if (position == null)
            Debug.LogError("Cannot position AlignedPointManagerUI as player has no assigned UIPosition component");
        else
        {
            gameObject.SetActive(visibility);
            Player.GetLocalPlayer().SetUIInteractorsEnabled(visibility, this.gameObject);
        }
    }

    public void ToggleUI()
    {
        SetUIVisibility(!this.gameObject.activeInHierarchy);
    }
}
