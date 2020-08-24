using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;
using GoogleARCore;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public string playerName;

    [SyncVar]
    public DevicePrefabMap.XRDeviceType deviceType;
    public Camera mainCamera;
    public GameObject RigSceneAnchorOffset;
    public GameObject Root;
    public List<XRBaseInteractor> UIInteractors;
    private List<GameObject> ActiveUIsInScene = new List<GameObject>();
    public bool ToggleLocalNonLocalScripts = true;

    public void SetUIInteractorsEnabled(bool enabled, GameObject UI)
    {
        if (UI != null)
        {
            if (enabled && !ActiveUIsInScene.Contains(UI)) ActiveUIsInScene.Add(UI);
            else if (!enabled) ActiveUIsInScene.Remove(UI);
        }
        UIInteractors.ForEach(x => x.gameObject.SetActive(ActiveUIsInScene.Count > 0));   
    }

    void Awake()
    {
        Debug.Log("Player Awake, isLocalPlayer=" + isLocalPlayer + " deviceType=" + deviceType);
        DontDestroyOnLoad(this.gameObject);
        if (isLocalPlayer)
            deviceType = FindObjectOfType<DevicePrefabMap>().GetThisDeviceType();

        Players.Add(this);
        mainCamera = GetComponentInChildren<Camera>(); 
        SetUIInteractorsEnabled(false, null);

        if (isLocalPlayer)
        {
            List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances<XRInputSubsystem>(subsystems);
            for (int i = 0; i < subsystems.Count; i++)
            {
                subsystems[i].TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            }
        }
    }

    public void Start()
    {
        if (this.isLocalPlayer)
        {
            playerName = "Player " + Random.Range(0, 100000);
        }

        if (ToggleLocalNonLocalScripts)
            LocalNonLocalSetup();
    }

    public void OnDestroy()
    {
        Players.Remove(this);
    }

    [Command]
    public void CmdChangeScene(string scene)
    {
        Debug.Log("CmdChangeScene called for scene " + scene);
        CollocatedNetworkManager manager = FindObjectOfType<CollocatedNetworkManager>();
        manager.ServerChangeScene(scene);
    }

    void DisableType<T>(bool inChildren = true) where T : Behaviour
    {
        foreach (T comp in  (inChildren ? GetComponentsInChildren<T>() : GetComponents<T>()))
        {
            comp.enabled = false;
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    void LocalNonLocalSetup()
    {
        if (GetComponent<OVRCameraRig>() != null) GetComponent<OVRCameraRig>().enabled = isLocalPlayer;
        if (GetComponent<OVRManager>() != null) GetComponent<OVRManager>().enabled = isLocalPlayer;
        if (GetComponent<OVRHeadsetEmulator>() != null) GetComponent<OVRHeadsetEmulator>().enabled = false;
        if (GetComponent<TrackingFailureDetector>() != null) GetComponent<TrackingFailureDetector>().enabled = false;

        if (GetComponentInChildren<Camera>() != null) GetComponentInChildren<Camera>().enabled = isLocalPlayer;
        this.mainCamera.enabled = isLocalPlayer;
        if (GetComponentInChildren<AudioListener>() != null) GetComponentInChildren<AudioListener>().enabled = isLocalPlayer;
        if (GetComponentInChildren<TrackedPoseDriver>() != null) GetComponentInChildren<TrackedPoseDriver>().enabled = isLocalPlayer;
        GetComponentsInChildren<XRController>()?.ToList().ForEach(x => x.enabled = isLocalPlayer);

        // only allow to remain enabled if it was intended to be enabled at startup
        ARCoreBackgroundRenderer arcoreRenderer = GetComponentInChildren<ARCoreBackgroundRenderer>();
        if (arcoreRenderer != null) arcoreRenderer.enabled = isLocalPlayer ? arcoreRenderer.enabled : false;


        if (FindObjectsOfType<ARCoreSession>().ToList().Where(x => x.enabled).Count() == 0)
        {
            ARCoreSession playerSession = GetComponent<ARCoreSession>();
            if (isLocalPlayer) {
                if (playerSession != null)
                    playerSession.enabled = isLocalPlayer;
            } else
            {
                // don't need more than 1 session!
                DestroyImmediate(playerSession);
            }

        }

        CheckLocalRemoteTagsRecursive(this.gameObject);
    }

    private void CheckLocalRemoteTagsRecursive(GameObject obj)
    {
        if (null == obj)
            return;

        if ((obj.CompareTag("LocalOnly") && !isLocalPlayer) ||
            (obj.name.Contains("Local") && !isLocalPlayer) )
            obj.SetActive(false);

        if ((obj.CompareTag("RemoteOnly") && isLocalPlayer) ||
            (obj.name.Contains("Remote") && isLocalPlayer))
            obj.SetActive(false);

        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;

            CheckLocalRemoteTagsRecursive(child.gameObject);
        }
    }

    private static bool AlwaysReturnPlayer = true;
    public static Player GetLocalPlayer()
    {
        if (Players.Count == 0)
            return null;

        if (FindObjectOfType<NetworkManager>() == null)
            return Players.First();
        else
        {
            Player Player = Players.SingleOrDefault(x => x.isLocalPlayer);
            if (Player == null && AlwaysReturnPlayer)
                Player = Players.First();
            return Player;
        }
    }

    private static Camera playerCamera;
    public static Camera GetLocalPlayerCamera()
    {
        if (playerCamera == null)
        {
            if (Player.GetLocalPlayer() != null)
                playerCamera = Player.GetLocalPlayer().mainCamera;
            else
                playerCamera = Camera.main;
        }

        return playerCamera;
    }


    public static List<Player> Players = new List<Player>();


    private AudioSource audioSource;
    public void PlayAudioAlert(AudioClip alert)
    {
        if (audioSource == null)
        {
            audioSource = this.mainCamera.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = this.mainCamera.gameObject.AddComponent<AudioSource>();
        }

        if (alert != null)
        {
            audioSource.loop = false;
            audioSource.PlayOneShot(alert);
        }
    }
}