using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// This provides networked support for Oculus Avatars in Mirror.
/// 
/// It synchronises the OvrAvatar packet data across server and clients, 
/// allowing visibility e.g. of avatar appearances, hands, lip movements etc.
/// </summary>
public class MirrorOvrAvatar : NetworkBehaviour
{
    public OvrAvatar ovrAvatar;
    public OvrAvatarRemoteDriver remoteDriver;

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<Player>().ToggleLocalNonLocalScripts)
        {
            if (isLocalPlayer)
            {
                remoteDriver.gameObject.SetActive(false);
                ovrAvatar.gameObject.SetActive(true);
                ovrAvatar.RecordPackets = true;
                ovrAvatar.PacketRecorded += OnLocalAvatarPacketRecorded;
            }
            else
            {
                remoteDriver.gameObject.SetActive(true);
                ovrAvatar.gameObject.SetActive(false);
            }
        }
    }

    public void OnDisable()
    {
        if (isLocalPlayer)
        {
            ovrAvatar.RecordPackets = false;
            ovrAvatar.PacketRecorded -= OnLocalAvatarPacketRecorded;
        }
    }

    /// <summary>
    /// Tracks the current packet number for in-order processing
    /// </summary>
    private int localSequence;

    /// <summary>
    /// Invoked on local player when they have a new Avatar packet to be sent.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void OnLocalAvatarPacketRecorded(object sender, OvrAvatar.PacketEventArgs args)
    {
        using (MemoryStream outputStream = new MemoryStream())
        {
            BinaryWriter writer = new BinaryWriter(outputStream);

            var size = Oculus.Avatar.CAPI.ovrAvatarPacket_GetSize(args.Packet.ovrNativePacket);
            byte[] data = new byte[size];
            Oculus.Avatar.CAPI.ovrAvatarPacket_Write(args.Packet.ovrNativePacket, size, data);

            writer.Write(localSequence++);
            writer.Write(size);
            writer.Write(data);

            byte[] output = outputStream.ToArray();
            // Debug.Log("Invoking CmdNewAvatarPacket with packet of length " + output.Length);
            CmdNewAvatarPacket(output);
            //Debug.Log("Wrote OvrAvatar packet to list, count now " + packetData.Count + " wrote array of bytes " + output.Length);
        }
    }

    /// <summary>
    /// Process the packet server side
    /// </summary>
    /// <param name="packet"></param>
    [Command]
    public void CmdNewAvatarPacket(byte[] packet)
    {
        // Debug.Log("Server got packet of length " + packet.Length);
        if (packet.Length > 0)
        {
            DeserializeAndQueuePacketData(packet);
            //packetData = Encoding.BigEndianUnicode.GetString(packet);
            RpcNewAvatarPacket(packet);
        }
    }

    /// <summary>
    /// Process the packet on other clients
    /// </summary>
    /// <param name="packet"></param>
    [ClientRpc]
    public void RpcNewAvatarPacket(byte[] packet)
    {
        DeserializeAndQueuePacketData(packet);
    }

    private void DeserializeAndQueuePacketData(byte[] data)
    {
        if (!isLocalPlayer)
        {
            // Debug.Log("Client got packet of length " + data.Length);

            using (MemoryStream inputStream = new MemoryStream(data))
            {
                BinaryReader reader = new BinaryReader(inputStream);
                int remoteSequence = reader.ReadInt32();

                int size = reader.ReadInt32();
                byte[] sdkData = reader.ReadBytes(size);

                System.IntPtr packet = Oculus.Avatar.CAPI.ovrAvatarPacket_Read((System.UInt32)data.Length, sdkData);
                remoteDriver.QueuePacket(remoteSequence, new OvrAvatarPacket { ovrNativePacket = packet });
            }
        }
    }
}
