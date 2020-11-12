using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button goButton;

    public static string DisplayName { get; private set; }

    private const string PlayerPrefsNameKey = "PlayerName";

    private void Start() => SetUpInputField();

    private void SetUpInputField() {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) return;

        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);

        input.text = defaultName;

        SetPlayerName(defaultName);
    }

    public void SetPlayerName(string name) {
        goButton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName() {
        DisplayName = input.text;
        PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
    }
}
