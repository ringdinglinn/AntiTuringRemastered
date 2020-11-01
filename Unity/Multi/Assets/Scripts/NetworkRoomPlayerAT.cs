using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class NetworkRoomPlayerAT : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[15];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[15];
    [SerializeField] private Button startGameButton;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    private bool isLeader;

    public override void OnStartAuthority() {
        CmdSetDisplayName(PlayerNameInput.DisplayName);
        lobbyUI.SetActive(true);
    }

    public bool IsLeader {
        set {
            isLeader = value;
            startGameButton.gameObject.SetActive(true);
        }
    }

    private NetworkManagerAT room;

    // this returns and sometimes assigns the network manager to out player
    // maybe this is because we can't assign it to the prefab, since it exists in the scene
    public NetworkManagerAT Room {
        get {
            if (room != null) return room;
            return room = NetworkManager.singleton as NetworkManagerAT;
        }
    }

    public override void OnStartClient() {
        Room.RoomPlayers.Add(this);
        UpdateDisplay();
    }

    public override void OnNetworkDestroy() {
        Room.RoomPlayers.Remove(this);
        UpdateDisplay();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    private void UpdateDisplay() {
        if (!isLocalPlayer) {
            // i dont understand this. this method is called by a sync var. if it is called
            // on all clients then we have to check if this is the local player so it isn't
            // called on each several times? but then we still have to execute on the local player
            // if it is called on everyone, why wouldn't it execute once for each as they are their
            // local players?
            foreach (var player in Room.RoomPlayers) {
                if (player.isLocalPlayer) {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }

        // clear all player names and ready states
        for (int i = 0; i < playerNameTexts.Length; i++) {
            playerNameTexts[i].text = "Waiting for player...";
            playerReadyTexts[i].text = string.Empty;
        }

        //for all the players set their current names and ready states
        for (int i = 0; i < Room.RoomPlayers.Count; i++) {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart) {
        if (!isLeader) return;

        startGameButton.interactable = readyToStart;
    }


    [Command]
    private void CmdSetDisplayName(string displayName) {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp() {
        IsReady = !IsReady;

        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame() {
        // Server ensures that this client connection is the leader
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) return;
        Room.StartGame();
    }
} 
