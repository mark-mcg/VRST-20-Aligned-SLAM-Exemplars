using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this if the server should be able to remotely perform alignments
/// </summary>
[RequireComponent(typeof(AlignedPointManager))]
public class NetworkedAlignedPointManager : NetworkBehaviour
{
    public AlignedPointManager alignedPointManager;

    public void Start()
    {
        if (alignedPointManager == null)
            alignedPointManager = GetComponent<AlignedPointManager>();
    }

    /// <summary>
    /// If the server is to assign the transformation point remotely (e.g. for audiences with assigned seating and headsets) 
    /// </summary>
    /// <param name="anchor"></param>
    [ClientRpc]
    void RpcAlignTo(string anchor)
    {
        alignedPointManager?.AlignTo(anchor);
    }
}
