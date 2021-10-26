using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MLAPI;
using MLAPI.NetworkVariable;
using System;

public class CarOverheadDisplay : NetworkBehaviour
{
    [SerializeField]
    private Canvas displayCanvas;
    [SerializeField]
    private TextMeshProUGUI displayNameText;

    private NetworkVariableString displayName = new NetworkVariableString();

    public override void NetworkStart()
    {
        if (!IsServer) return;

        // PlayerData? playerData = PasswordNetworkManager.GetPlayerData(OwnerClientId);
        PlayerData? playerData = ServerGameNetworkPortal.Instance.GetPlayerData(OwnerClientId);
        if (playerData.HasValue)
        {
            displayName.Value = playerData.Value.PlayerName;
        }
    }

    private void Start()
    {
        displayCanvas.worldCamera = Camera.main;
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    private void OnEnable()
    {
        displayName.OnValueChanged += HandleDisplayNameChanged;
    }

    private void OnDisable()
    {
        displayName.OnValueChanged -= HandleDisplayNameChanged;
    }

    private void HandleDisplayNameChanged(string oldName, string newName)
    {
        displayNameText.text = newName;
    }
}
