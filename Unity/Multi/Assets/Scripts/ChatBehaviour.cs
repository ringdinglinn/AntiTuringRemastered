using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;



public class ChatBehaviour : NetworkBehaviour {
    [SerializeField] private List<TMP_Text> chatDisplays;
    [SerializeField] private List<TMP_InputField> inputFields;
    [SerializeField] private GameObject chatUI;

    private NetworkGamePlayerAT networkPlayer;

    private static event Action<String, int> OnMessage;

    private int chatroomID;

    public override void OnStartAuthority() {
        chatUI.SetActive(true);
        OnMessage += HandleNewMessage;
    }

    public override void OnStartClient() {
        base.OnStartClient();
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
    }

    [ClientCallback]
    private void OnDestroy() {
        if (!hasAuthority) return;

        OnMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(string message, int i) {
        chatDisplays[i].text += message;
    }

    [Client]
    public void Send(string message) {
        if (string.IsNullOrWhiteSpace(message)) return;
        CmdSendMessage(message, chatroomID);

        inputFields[chatroomID].text = string.Empty;
    }

    [Command]
    private void CmdSendMessage(string message, int chatroomID) {
        RpcHandleMessage($"[{networkPlayer.GetDisplayName()}]: {message}", chatroomID);
    }

    [ClientRpc]
    private void RpcHandleMessage(string message, int chatroomID) {
        OnMessage?.Invoke($"\n{message}", chatroomID);
    }

    [Client]
    public void SelectChatroom(string s) {
        chatroomID = int.Parse(EventSystem.current.currentSelectedGameObject.name);
    }
}
