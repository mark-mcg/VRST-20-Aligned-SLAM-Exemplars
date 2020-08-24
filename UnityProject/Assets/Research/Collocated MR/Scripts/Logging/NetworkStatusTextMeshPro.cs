using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkStatusTextMeshPro : MonoBehaviour
{
    public TMPro.TMP_Text  text;
    CollocatedNetworkManager networkManager;

    // Start is called before the first frame update
    void Start()
    {
        networkManager = FindObjectOfType<CollocatedNetworkManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (networkManager != null)
        {
            text.text = "Offline, connecting to " + networkManager.networkAddress + " server=" + networkManager.Server + " client=" + networkManager.Client;
        }
    }
}
