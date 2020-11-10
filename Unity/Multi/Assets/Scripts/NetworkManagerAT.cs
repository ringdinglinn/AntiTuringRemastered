using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkManagerAT : NetworkManager {
    [SerializeField] private int minPlayers = 2;
    [Scene] [SerializeField] private string menuScene;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerAT roomPlayerPrefab;

    [Header("Game")]
    [SerializeField] private NetworkGamePlayerAT gamePlayerPrefab;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;

    public List<NetworkRoomPlayerAT> RoomPlayers { get; } = new List<NetworkRoomPlayerAT>();
    public List<NetworkGamePlayerAT> GamePlayers { get; } = new List<NetworkGamePlayerAT>();
    private bool[] roleIndex;

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList<GameObject>();

    private int nrInvestigators;
    private int nrAwareAI;
    private int nrPlayers;

    public ChatbotBehaviour chatbot;

    public override void OnStartClient() {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (GameObject prefab in spawnablePrefabs) {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn) {
        Debug.Log("on client connect");
        Debug.Log(conn.connectionId);
        base.OnClientConnect(conn);
        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn) {
        base.OnClientDisconnect(conn);
        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn) {
        Debug.Log("on server connect");
        if (numPlayers >= maxConnections) {
            conn.Disconnect();
            return;
        }

        if ("Assets/Scenes/" + SceneManager.GetActiveScene().name + ".unity" != menuScene) {
            conn.Disconnect();
            return;
        }
    }

    // is called in OnStartClient
    public override void OnServerAddPlayer(NetworkConnection conn) {
        if ("Assets/Scenes/" + SceneManager.GetActiveScene().name + ".unity" == menuScene) {
            bool isLeader = RoomPlayers.Count == 0;
            NetworkRoomPlayerAT roomPlayerInstance = Instantiate(roomPlayerPrefab);
            roomPlayerInstance.IsLeader = isLeader;
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn) {
        if (conn.identity != null) {
            var player = conn.identity.GetComponent<NetworkRoomPlayerAT>();
            RoomPlayers.Remove(player);
            NotifyPlayersOfReadyState();
        }
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer() {
        RoomPlayers.Clear();
    }

    public void NotifyPlayersOfReadyState() {
        foreach (var player in RoomPlayers) {
            player.HandleReadyToStart(IsReadyToStart());  
        }
    }

    private bool IsReadyToStart() {
        if (numPlayers < minPlayers) return false;

        foreach(var player in RoomPlayers) {
            if (!player.IsReady) return false;
        }

        return true;
    }

    public void StartGame() {
        if (SceneManager.GetActiveScene().path == menuScene) {
            if (!IsReadyToStart()) return;

            nrPlayers = RoomPlayers.Count;
            nrAwareAI = nrPlayers - nrInvestigators;
            ServerChangeScene("SampleScene");
        }
    }


    public override void ServerChangeScene(string newSceneName) {
        if (SceneManager.GetActiveScene().path == menuScene && newSceneName == "SampleScene") {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--) {
                var conn = RoomPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);
                gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
                
                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
            }
        }
        base.ServerChangeScene(newSceneName);
    }

    public bool GetRole(int i) {
        return roleIndex[i];
    }

    public void SetRoleIndex(bool[] newIndex) {
        Debug.Log("SetRoleIndex()");
        roleIndex = new bool[RoomPlayers.Count];
        roleIndex = newIndex;
    }

    public void SetNrInvestigators(int n) {
        nrInvestigators = n;
    }
}

