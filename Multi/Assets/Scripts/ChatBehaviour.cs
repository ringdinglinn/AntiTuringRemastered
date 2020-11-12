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
    private Dictionary<int, bool> chatbotRoomsIndex = new Dictionary<int, bool>();

    //botname placeholder
    string botname = "???";

    public override void OnStartAuthority() {
        chatbotRoomsIndex[0] = true;
        chatUI.SetActive(true);
        OnMessage += HandleNewMessage;
        networkPlayer = GetComponent<NetworkGamePlayerAT>();
        chatbotRoomsIndex[0] = true;
        for (int i = 1; i < inputFields.Count; i++) chatbotRoomsIndex.Add(i, false);
        foreach (KeyValuePair<int, bool> kvp in chatbotRoomsIndex) Debug.Log(kvp.Key + ", " + kvp.Value);
    }

    public override void OnStartClient() {
        base.OnStartClient();

    }

    [ClientCallback]
    private void OnDestroy() {
        if (!hasAuthority) return;

        OnMessage -= HandleNewMessage;
    }

    private void HandleNewMessage(string message, int i) {
        Debug.Log("handle new message event");
        Debug.Log(chatDisplays[i].text.Replace("\n", "  "));
        chatDisplays[i].text += message;
        Debug.Log(chatDisplays[i].text.Replace("\n", "  "));
    }

    [Client]
    public void Send(string message) {

        Debug.Log("message = " + message + ", chatroomID = " + chatroomID);
        if (string.IsNullOrWhiteSpace(message)) return;

        CmdSendMessage(message, chatroomID, networkPlayer.GetDisplayName());

        Debug.Log(chatroomID);
        foreach (KeyValuePair<int, bool> kvp in chatbotRoomsIndex) Debug.Log(kvp.Key + ", " + kvp.Value);
        if (chatbotRoomsIndex[chatroomID]) CmdSendMessageToChatbot(message);


        inputFields[chatroomID].text = string.Empty;
    }

    [Command]
    private void CmdSendMessage(string message, int chatroomID, string name) {
        RpcHandleMessage($"[{name}]: {message}", chatroomID);
    }

    [ClientRpc]
    private void RpcHandleMessage(string message, int chatroomID) {
        Debug.Log("rpc handle message: " + message);
        OnMessage?.Invoke($"\n{message}", chatroomID);
    }

    [Client]
    public void SelectChatroom(string s) {
        chatroomID = int.Parse(EventSystem.current.currentSelectedGameObject.name);
    }

    [Command]
    public void CmdSendMessageToChatbot(string text) {
        networkPlayer.Room.chatbot.SendTextToChatbot(text, chatroomID);
    }

    [Command]
    public void CmdSendOutResponseFromChatbot(string r, int id) {
        Debug.Log("CmdSendOutResponseFromChatbot");
        RpcHandleMessage(r, id);
    }

    public void ReceiveChatbotMessageFromPlayer(string r, int id) {
        r = $"[{botname}]: {r}";
        if (isClientOnly) {
            CmdSendOutResponseFromChatbot(r, id);
            return;
        }
        RpcHandleMessage(r, id);
    }
}
