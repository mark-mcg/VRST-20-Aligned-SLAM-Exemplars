using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Hooks into an XRGrabInteractable to monitor select events for network synchronization.
/// </summary>
public class NetworkXRGrabInteractable : NetworkBehaviour
{
    private XRGrabInteractable grabInteractable;
    private NetworkXRInteractionManagerExtension manager;

    public class SelectMessage : MessageBase
    {
        [Serializable]
        public enum SelectionState { SelectEntry, SelectExit };
        public SelectionState selectionState;
        public string interactorName;
        public GameObject interactable;
        public GameObject player;

        public SelectMessage SetMessage(SelectionState state, XRBaseInteractor interactor, XRGrabInteractable interactable, Player player)
        {
            this.selectionState = state;
            this.interactorName = interactor.gameObject.name;
            this.interactable = interactable.gameObject;
            this.player = player.gameObject;
            return this;
        }

        public override string ToString()
        {
            return "State " + selectionState + " for player " + player + " using interactor " + interactorName + " on interactable " + interactable;
        }

        // interactors are children of players, so can't have their own netids, and therefore we can't share them as GameObjects
        public XRBaseInteractor GetInteractor()
        {
            return player.GetComponentsInChildren<XRBaseInteractor>().SingleOrDefault(x => x.gameObject.name.Equals(interactorName));
        }

        public XRGrabInteractable GetInteractable()
        {
            return interactable.GetComponent<XRGrabInteractable>();
        }

        public Player GetPlayer()
        {
            return player.GetComponent<Player>();
        }
    }

    private void Awake()
    {
        manager = FindObjectOfType<NetworkXRInteractionManagerExtension>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.onSelectEnter.AddListener(OnSelectEnter);
        grabInteractable.onSelectExit.AddListener(OnSelectExit);
    }

    #region Client side - Selection events
    protected void OnSelectEnter(XRBaseInteractor interactor)
    {
        Player player = interactor.GetComponentInParent<Player>();
        if (player.isLocalPlayer)
        {
            SelectMessage grabMessage = new SelectMessage().SetMessage(
                SelectMessage.SelectionState.SelectEntry,
                interactor, grabInteractable, Player.GetLocalPlayer());

            NetworkClient.Send<SelectMessage>(grabMessage);
        }
    }

    protected void OnSelectExit(XRBaseInteractor interactor)
    {
        Player player = interactor.GetComponentInParent<Player>();
        if (player.isLocalPlayer)
        {
            SelectMessage grabMessage = new SelectMessage().SetMessage(
                SelectMessage.SelectionState.SelectExit,
                interactor, grabInteractable, Player.GetLocalPlayer());

            NetworkClient.Send<SelectMessage>(grabMessage);
        }
    }
    #endregion
}
