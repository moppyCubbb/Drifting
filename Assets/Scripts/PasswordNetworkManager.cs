using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class PasswordNetworkManager : MonoBehaviour
{
    [SerializeField]
    private InputField passwordInputField;

    [SerializeField]
    private InputField nameInputField;

    [SerializeField]
    private GameObject passwordEntryUI;

    [SerializeField]
    private GameObject leaveButton;

    [SerializeField]
    private GameObject teamPicker;

    private static Dictionary<ulong, PlayerData> clientData;

    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;


        NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public void Host()
    {
        clientData = new Dictionary<ulong, PlayerData>();
        // add Host itself to client data
        clientData[NetworkManager.Singleton.LocalClientId] = new PlayerData(nameInputField.text, NetworkManager.Singleton.LocalClientId);

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost(new Vector3(-2, 0, 0));
        
    }

    public void Client()
    {
        var payload = JsonUtility.ToJson(new ConnectionPayload ()
        {
            //password = passwordInputField.text,
            playerName = nameInputField.text,
        });

        byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartClient();
    }

    public void Leave()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StopHost();
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }

        passwordEntryUI.SetActive(true);
        leaveButton.SetActive(false);
        teamPicker.SetActive(false);
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        //string payload = Encoding.ASCII.GetString(connectionData);
        string payload = Encoding.UTF8.GetString(connectionData);
        ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

        // check the password
        //bool approveConnection = connectionPayload.password == passwordInputField.text;
        bool approveConnection = true;

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (approveConnection)
        {
            switch (NetworkManager.Singleton.ConnectedClients.Count)
            {
                case 1:
                    spawnPosition = new Vector3(0, 0, 0);
                    break;
                case 2:
                    spawnPosition = new Vector3(2, 0, 0);
                    break;
            }

            clientData[clientId] = new PlayerData(connectionPayload.playerName, clientId);
        }
        
        
        callback(true, null, approveConnection, spawnPosition, spawnRotation);
    }

    private void HandleServerStarted()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            // for host, connect by themselves
            HandleClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }

    // called on the server when there is a client join
    // also called on the client that join but not other existing client
    private void HandleClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("client connected");
            passwordEntryUI.SetActive(false);
            leaveButton.SetActive(true);
            teamPicker.SetActive(true);
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            clientData.Remove(clientId);
        }
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            passwordEntryUI.SetActive(true);
            leaveButton.SetActive(false);
            teamPicker.SetActive(false);
        }
    }

    public static PlayerData? GetPlayerData(ulong clientId)
    {
        if (clientData.TryGetValue(clientId, out PlayerData playerData))
        {
            return playerData;
        }

        return null;
    }
}
