using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static NetworkXRGrabInteractable;

/// <summary>
/// Synchronizes select events across server and clients, so that when an object is grabbed, the client that grabbed it 
/// can move the object in realtime, and the server/clients syncrhonize these movements.
/// 
/// Operates on a last come first server basis i.e. if a player enacts a grab, the current grabber loses authority on the object.
/// 
/// N.B. This receives messages from NetworkXRGrabInteractables.
/// </summary>
[RequireComponent(typeof(XRInteractionManager))]
public class NetworkXRInteractionManagerExtension : MonoBehaviour
{
    XRInteractionManager baseManager;

    public void Awake()
    {
        baseManager = GetComponent<XRInteractionManager>();
        NetworkServer.RegisterHandler<SelectMessage>(OnSelectEventServerSide);
        NetworkClient.RegisterHandler<SelectMessage>(OnSelectEventClientSide);
    }

    #region Server side - reacting to initial client event of selection entry/exit
    protected void OnSelectEventServerSide(SelectMessage grabMsg)
    {
        Debug.Log("SERVER (OnSelectEventServerSide) " + grabMsg);
        NetworkIdentity newOwner = grabMsg.GetPlayer().GetComponent<NetworkIdentity>();
        // NetworkIdentity.spawned.TryGetValue(grabMsg.selectedByNetID, out newOwner);

        if (grabMsg.selectionState == SelectMessage.SelectionState.SelectEntry)
        {
            Debug.Log("SERVER (OnSelectEventServerSide) Removing authority for " + grabMsg.GetInteractable().name + " and assigning new owner " + newOwner + " id " + newOwner.netId + " using connection " + newOwner.connectionToClient);
            grabMsg.GetInteractable().GetComponent<NetworkIdentity>().RemoveClientAuthority();
            grabMsg.GetInteractable().GetComponent<NetworkIdentity>().AssignClientAuthority(newOwner.connectionToClient);
        }
        else
        {
            if (grabMsg.GetInteractable().selectingInteractor.gameObject == grabMsg.GetInteractor())
            {
                Debug.Log("SERVER (OnSelectEventServerSide) Removing authority, end of interaction for " + grabMsg.GetInteractable().name);
                grabMsg.GetInteractable().GetComponent<NetworkIdentity>().RemoveClientAuthority();
            }
        }

        // update server state
        OnSelectEventClientSide(grabMsg);

        // update the clients about this event
        NetworkServer.SendToAll<SelectMessage>(grabMsg);
    }
    #endregion

    #region Client side - other clients being informed of the selection entry/exit by server
    protected void OnSelectEventClientSide(SelectMessage grabMsg)
    {
        Debug.Log("CLIENT/SERVER (OnSelectEventClientSide) " + grabMsg);
        if (Player.GetLocalPlayer() == null || (Player.GetLocalPlayer() != null && grabMsg.GetPlayer() != Player.GetLocalPlayer()))
        {
            if (grabMsg.selectionState == SelectMessage.SelectionState.SelectEntry)
            {
                SelectEnter(grabMsg.GetInteractor(), grabMsg.GetInteractable());
            }
            else
            {
                SelectExit(grabMsg.GetInteractor(), grabMsg.GetInteractable());
            }
        }
    }
    #endregion

    #region XRInteractionManager invocations
    /*
        * This is a tricky bit - we ideally want to extend XRInteractionManager, but we can't
        * e.g. the methods we want to override aren't virtual (e.g. listening to selection events
        * on active interactables), nor are the methods we want to invoke (e.g. SelectEnter on
        * a remote client) visible.
        * 
        * Instead, we use reflection to manually call SelectEnter/SelectExit as appropriate.
        * 
        * N.B. If these methods or their signatures change (e.g. for future versions of
        * the interaction toolkit) this will break!
        */
    private MethodInfo SelectExitMethod;

    public void SelectExit(XRBaseInteractor interactor, XRBaseInteractable interactable)
    {
        if (SelectExitMethod == null)
        {
            System.Type ourType = baseManager.GetType();
            SelectExitMethod = ourType.GetMethod("SelectExit", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        if (SelectExitMethod != null)
        {
            SelectExitMethod.Invoke(baseManager, new object[] { interactor, interactable });
        }
        else
        {
            Debug.LogError("Can't invoke SelectExit on XRInteractionManager, interactions will not be synced over the network! Has the method or function changed in an update?");
        }
    }

    private MethodInfo SelectEnterMethod;
    public void SelectEnter(XRBaseInteractor interactor, XRBaseInteractable interactable)
    {
        if (SelectEnterMethod == null)
        {
            System.Type ourType = baseManager.GetType();
            SelectEnterMethod = ourType.GetMethod("SelectEnter", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        if (SelectEnterMethod != null)
        {
            SelectEnterMethod.Invoke(baseManager, new object[] { interactor, interactable });
        } else
        {
            Debug.LogError("Can't invoke SelectEnter on XRInteractionManager, interactions will not be synced over the network! Has the method or function changed in an update?");
        }
    }
    #endregion
}
