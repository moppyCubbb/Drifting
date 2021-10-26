using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LobbyPlayerCard : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject waitingForPlayerPanel;
    [SerializeField]
    private GameObject playerDataPanel;

    [Header("Data Display")]
    [SerializeField]
    private TMP_Text playerDisplayNameText;
    [SerializeField]
    private Image selectedCharacterImage;
    [SerializeField]
    private Toggle isReadyToggle;

    public void UpdateDisplay(NetworkLobbyPlayerState networkLobbyPlayerState)
    {

        playerDisplayNameText.text = networkLobbyPlayerState.PlayerName;
        isReadyToggle.isOn = networkLobbyPlayerState.IsReady;
        waitingForPlayerPanel.SetActive(false);
        playerDataPanel.SetActive(true);
    }

    public void DisableDisplay()
    {
        waitingForPlayerPanel.SetActive(true);
        playerDataPanel.SetActive(false);
    }
}
