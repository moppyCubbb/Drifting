using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Spawning;

[RequireComponent(typeof(GameNetworkPortal))]
public class ServerGameNetworkPortal : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private int maxPlayer = 4;

    public static ServerGameNetworkPortal Instance => instance;
    private static ServerGameNetworkPortal instance;

    private Dictionary<string, PlayerData> clientData;
    private Dictionary<ulong, string> clientIdToGuid;
    private Dictionary<ulong, int> clientSceneMap;
    private bool gameInProgress;

    private const int MaxConnectionPayload = 1024;

    private GameNetworkPortal gameNetworkPortal;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        gameNetworkPortal = GetComponent<GameNetworkPortal>();
        gameNetworkPortal.OnNetworkReadied += HandleNetworkReadied;

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += HandlerServerStarted;

        clientData = new Dictionary<string, PlayerData>();
        clientIdToGuid = new Dictionary<ulong, string>();
        clientSceneMap = new Dictionary<ulong, int>();
    }

    private void OnDestroy()
    {
        if (gameNetworkPortal == null) return;

        gameNetworkPortal.OnNetworkReadied -= HandleNetworkReadied;

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted -= HandlerServerStarted;
    }

    public void StartGame()
    {
        gameInProgress = true;

        NetworkSceneManager.SwitchScene("PlayerScene");
    }

    public void EndRound()
    {
        gameInProgress = false;

        NetworkSceneManager.SwitchScene("NetworkLobbyScene");
    }

    public PlayerData? GetPlayerData(ulong clientId)
    {
        if (clientIdToGuid.TryGetValue(clientId, out string clientGuid))
        {
            if (clientData.TryGetValue(clientGuid, out PlayerData playerData))
            {
                return playerData;
            }
            else
            {
                Debug.LogWarning($"No player data found for client id: {clientId}");
            }
        }
        else
        {
            Debug.LogWarning($"No client guid found for client id: {clientId}");
        }

        return null;
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        if (connectionData.Length > MaxConnectionPayload)
        {
            callback(false, 0, false, null, null);
            return;
        }

        string payload = Encoding.ASCII.GetString(connectionData);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

        // check game status
        ConnectionStatus gameReturnStatus = ConnectionStatus.Success;
        if (gameInProgress)
        {
            gameReturnStatus = ConnectionStatus.GameInProgress;
        }
        else if (clientData.Count == maxPlayer)
        {
            gameReturnStatus = ConnectionStatus.ServerFull;
        }

        // check password
        /*if (connectionPayload.usePassword)
        {
            string password = connectionPayload.password;
            bool approveConnection = connectionPayload.password == passwordInputField.text;
        }*/
        
        // if success, add client to clientSceneMap, clientIdToGuid and clientData
        if (gameReturnStatus == ConnectionStatus.Success)
        {
            clientSceneMap[clientId] = connectionPayload.clientScene;
            clientIdToGuid[clientId] = connectionPayload.clientGuid;
            clientData[connectionPayload.clientGuid] = new PlayerData(connectionPayload.playerName, clientId);
        }

        callback(false, 0, true, null, null);
        gameNetworkPortal.ServerToClientConnectResult(clientId, gameReturnStatus);
        
        if (gameReturnStatus != ConnectionStatus.Success)
        {
            StartCoroutine(WaitToDisconnectClient(clientId, gameReturnStatus));
        }
    }

    private IEnumerator WaitToDisconnectClient(ulong clientId, ConnectionStatus reason)
    {
        gameNetworkPortal.ServerToClientConnectResult(clientId, reason);
        yield return new WaitForSeconds(0);

        KickClient(clientId);
    }

    private void KickClient(ulong clientId)
    {
        NetworkObject networkObject = NetworkSpawnManager.GetPlayerNetworkObject(clientId);
        if (networkObject != null)
        {
            networkObject.Despawn(true);
        }

        NetworkManager.Singleton.DisconnectClient(clientId);
    }

    private void HandlerServerStarted()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        string clientGuid = Guid.NewGuid().ToString();
        string playerName = PlayerPrefs.GetString("PlayerName", "Missing Name");

        clientData.Add(clientGuid, new PlayerData(playerName, NetworkManager.Singleton.LocalClientId));
        clientIdToGuid.Add(NetworkManager.Singleton.LocalClientId, clientGuid);
    }

    private void HandleNetworkReadied()
    {
        // when the network is ready, add event for client disconnect and client scene change
        if (!NetworkManager.Singleton.IsServer) return;

        gameNetworkPortal.OnUserDisconnectRequested += HandleUserDisconnectRequested;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        gameNetworkPortal.OnClientSceneChanged += HandleClientSceneChanged;
        
        NetworkSceneManager.SwitchScene("NetworkLobbyScene");

        if (NetworkManager.Singleton.IsHost)
        {
            clientSceneMap[NetworkManager.Singleton.LocalClientId] = SceneManager.GetActiveScene().buildIndex;
        }
    }

    private void HandleClientSceneChanged(ulong clientId, int sceneIndex)
    {
        clientSceneMap[clientId] = sceneIndex;
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        clientSceneMap.Remove(clientId);
        if (clientIdToGuid.TryGetValue(clientId, out string guid))
        {
            clientIdToGuid.Remove(clientId);
            if (clientData[guid].ClientId == clientId)
            {
                clientData.Remove(guid);
            }
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            // remove all listen event when the client disconnect
            gameNetworkPortal.OnUserDisconnectRequested -= HandleUserDisconnectRequested;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            gameNetworkPortal.OnClientSceneChanged -= HandleClientSceneChanged;
        }
    }

    private void HandleUserDisconnectRequested()
    {
        HandleClientDisconnect(NetworkManager.Singleton.LocalClientId);
        NetworkManager.Singleton.StopHost();
        ClearData();
        SceneManager.LoadScene("NetworkMenuScene");
    }

    private void ClearData()
    {
        clientData.Clear();
        clientIdToGuid.Clear();
        clientSceneMap.Clear();

        gameInProgress = false;
    }
}
