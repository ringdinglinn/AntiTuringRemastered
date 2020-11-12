using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour {
    [SerializeField] private NetworkManagerAT networkManager;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel;

    public void HostLobby() {
        networkManager.StartHost();
        landingPagePanel.SetActive(false);
    }   
}
