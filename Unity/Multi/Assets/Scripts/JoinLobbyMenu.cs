using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField] private NetworkManagerAT networkManager;

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button joinButton;
    [SerializeField] private GameObject landingPagePanel;

    private void Start() {
        NetworkManagerAT.OnClientConnected += HandleClientConnected;
        NetworkManagerAT.OnClientDisconnected += HandleClientDisconnected;
        Debug.Log("listens to on client connected");
    }

    private void OnDisable() {
        NetworkManagerAT.OnClientConnected -= HandleClientConnected;
        NetworkManagerAT.OnClientDisconnected -= HandleClientDisconnected;
        Debug.Log("doesn't listen to on client connected");
    }

    public void JoinLobby() {
        string ipAddress = inputField.text;
        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected() {
        Debug.Log("handle client connected");
        joinButton.interactable = true;
        gameObject.SetActive(false);
        landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected() {
        joinButton.interactable = true;
    }
}
