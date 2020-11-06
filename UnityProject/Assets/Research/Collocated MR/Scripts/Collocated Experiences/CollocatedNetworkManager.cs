using Mirror;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System;

public class NetworkConfig
{
    public string ipaddress = "127.0.0.1";
}

/// <summary>
/// Basic NetworkManager implementation. Allows NetworkConfig (ipaddress) to be stored/retrived from JSON file on device.
/// </summary>
public class CollocatedNetworkManager : NetworkManager
{
    public class DeviceTypeMessage : MessageBase
    {
        public DevicePrefabMap.XRDeviceType deviceType;
    }

    [Header("QuestNetworkManager options")]
    public bool UseJSONNetworkConfig;
    public NetworkConfig networkConfig;
    public NetworkManager manager;

    public bool Client = false;
    public bool Server = false;

    public override void Awake()
    {
        base.Awake();
        if (manager == null) manager = FindObjectOfType<NetworkManager>();
        if (UseJSONNetworkConfig) LoadNetworkConfigFile(manager.networkAddress);
        autoCreatePlayer = false;
    }

    public override void Start()
    {
        base.Start();
        Connect();
    }

    protected void Connect()
    {
        Debug.LogFormat("Connect called, using address {0}", this.networkAddress); 
        if (Client && Server)
            StartHost();

        else if (Client)
            StartClient();

        else if (Server)
            StartServer();
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<DeviceTypeMessage>(OnCreatePlayer);

    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        Debug.Log("Client disconnected " + conn + " attempting to reconnect");

        // attempt to reconnect
        Connect();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log("Client connected " + conn + " with device " + SystemInfo.deviceModel + " on platform " + Application.platform);

        DevicePrefabMap prefabMap = FindObjectOfType<DevicePrefabMap>();

        // you can send the message here, or wherever else you want
        DeviceTypeMessage deviceMessage = new DeviceTypeMessage
        {
            deviceType = prefabMap.GetThisDeviceType()
        };

        Debug.Log("Determinxed XR client device is type " + prefabMap.GetThisDeviceType());
        conn.Send(deviceMessage);
    }

    [Header("Per-Device Prefab Types")]
    public DevicePrefabMap PlayerPrefabMap;

    void OnCreatePlayer(NetworkConnection conn, DeviceTypeMessage message)
    {
        DevicePrefabMap.PlayerPrefabType deviceTypePrefab = PlayerPrefabMap?.GetPrefabType(message.deviceType);
        GameObject prefab = deviceTypePrefab != null ? deviceTypePrefab.prefab : playerPrefab;

        if (prefab != null)
        {
            Debug.Log("Instantiating prefab " + prefab.name + " for new player");
            GameObject player = Instantiate(prefab);
            player.GetComponent<Player>().deviceType = message.deviceType;
            NetworkServer.AddPlayerForConnection(conn, player);

        } else
        {
            Debug.LogError("Can't identify a player prefab for device of type " + message.deviceType + " and no default prefab supplied!");
        }
    }

    internal void RestartClient()
    {
        if (Client)
        {
            StopClient();
            StartClient();
        }
    }

    private void LoadNetworkConfigFile(string transportIP)
    {
        // First load the config in if it exists, and set the connection ip address
        string filepath = FileHelpers.GetFileNameAndPath("config", "networking", false, ".json");
        Debug.Log("Using filepath " + filepath);
        JsonSerializer serializer = new JsonSerializer();

        Debug.Log("Trying to load network config from " + filepath);
        StreamReader networkConfigFile = FileHelpers.GetStreamReader(filepath);

        if (networkConfigFile != null)
        {
            JsonTextReader jsonReader = new JsonTextReader(networkConfigFile);
            networkConfig = (NetworkConfig)serializer.Deserialize(jsonReader, typeof(NetworkConfig));
            jsonReader.Close();
            networkConfigFile.Close();
        }


        // Then write the config back out (if it didn't already exist) so we can manually edit it later after first run
        if (networkConfig == null)
        {
            networkConfig = new NetworkConfig();
            networkConfig.ipaddress = transportIP;
        }

        this.networkAddress = networkConfig.ipaddress;

        StreamWriter streamWriter = FileHelpers.GetStreamWriter(filepath);
        if (streamWriter != null)
        {
            JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter);
            serializer.Serialize(jsonWriter, networkConfig);
            jsonWriter.Close();
            streamWriter.Close();
        }
        Debug.Log("Wrote network config to " + filepath);

    }
}
