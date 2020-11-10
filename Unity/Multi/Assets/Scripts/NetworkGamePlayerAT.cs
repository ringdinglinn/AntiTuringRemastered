using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class NetworkGamePlayerAT : NetworkBehaviour {
    [SyncVar]
    private string displayName = "Loading...";

    private NetworkManagerAT room;
    public ChatBehaviour chatBehaviour;

    public bool isInvestigator = false;

    public GameObject investigatorText;

    // this returns and sometimes assigns the network manager to out player
    // maybe this is because we can't assign it to the prefab, since it exists in the scene
    public NetworkManagerAT Room {
        get {
            if (room != null) return room;
            return room = NetworkManager.singleton as NetworkManagerAT;
        }
    }

    public override void OnStartClient() {
        DontDestroyOnLoad(gameObject);
        Room.GamePlayers.Add(this);
        isInvestigator = Room.GetRole(Room.GamePlayers.Count - 1);
        investigatorText.SetActive(isInvestigator);
    }

    public override void OnNetworkDestroy() {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName) {
        this.displayName = displayName;
    }

    public string GetDisplayName() {
        return displayName;
    }

    public void ReceiveMessageFromChatbot(string r, int id) {
        Debug.Log("received message :" + r);
        chatBehaviour.ReceiveChatbotMessageFromPlayer(r, id);
    }
} 
