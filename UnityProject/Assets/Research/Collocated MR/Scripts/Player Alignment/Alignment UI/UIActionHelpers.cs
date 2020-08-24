using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Helpers for enacting UI actions for smartphone/AR devices
/// </summary>
public class UIActionHelpers : MonoBehaviour
{
    /// <summary>
    /// For AR devices that can't determine their floor height at start.
    /// </summary>
    /// <param name="increment"></param>
    public void ModifyLocalPlayerHeight(float increment)
    {
        Vector3 pos = Player.GetLocalPlayer().RigSceneAnchorOffset.transform.localPosition;
        Player.GetLocalPlayer().RigSceneAnchorOffset.transform.localPosition = new Vector3(pos.x, pos.y + increment, pos.z);
    }

    public GameObject environment;
    /// <summary>
    /// For AR devices that wish to see players/actions but not the virtual environment (e.g. for MR viewing).
    /// </summary>
    public void ToggleEnvironmentVisibility()
    {
        if (environment == null)
            environment = GameObject.Find("Base Environment");

        if (environment != null)
        {
            environment.SetActive(!environment.activeInHierarchy);

            // for any AR devices, if we hide the background, show the camera feed
            ARCoreBackgroundRenderer backgroundRenderer = Player.GetLocalPlayer().GetComponentInChildren<ARCoreBackgroundRenderer>();
            if (backgroundRenderer != null)
                backgroundRenderer.enabled = !environment.activeInHierarchy;
        }
    }

    public List<string> scenesToCycleThrough = new List<string>();

    public void NextScene()
    {
        int currentScene = scenesToCycleThrough.IndexOf(SceneManager.GetActiveScene().name);
        Debug.Log("Current scene is: " + SceneManager.GetActiveScene().name + " index " + currentScene);
        currentScene++;
        if (currentScene > scenesToCycleThrough.Count - 1)
            currentScene = 0;

        CollocatedNetworkManager networkManager = FindObjectOfType<CollocatedNetworkManager>();
        Debug.Log("Changing scene to " + scenesToCycleThrough[currentScene]);
        Player.GetLocalPlayer().CmdChangeScene(scenesToCycleThrough[currentScene]);
    }
}
