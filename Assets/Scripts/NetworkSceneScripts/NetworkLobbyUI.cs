using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Connection;

public class NetworkLobbyUI : NetworkBehaviour
{
    [Header("References")]
    [SerializeField]
    private LobbyPlayerCard[] lobbyPlayerCards;
    [SerializeField]
    private Button startGameButton;

    private NetworkList<NetworkLobbyPlayerState> networkLobbyPlayers = new NetworkList<NetworkLobbyPlayerState>();

    public override void NetworkStart()
    {
        if (IsClient)
        {
            networkLobbyPlayers.OnListChanged += HandleNetworkLobbyPlayerStateChanged;
        }
        
        if (IsServer)
        {
            startGameButton.gameObject.SetActive(true);

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
    }

    private void OnDestroy()
    {
        networkLobbyPlayers.OnListChanged -= HandleNetworkLobbyPlayerStateChanged;

        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private bool IsEveryoneReady()
    {
        if (networkLobbyPlayers.Count < 2) return false;

        foreach (var player in networkLobbyPlayers)
        {
            if (!player.IsReady)
            {
                return false;
            }
        }
        return true;
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < networkLobbyPlayers.Count; i++)
        {
            if (networkLobbyPlayers[i].ClientId == clientId)
            {
                networkLobbyPlayers.RemoveAt(i);
                break;
            }
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        var playerData = ServerGameNetworkPortal.Instance.GetPlayerData(clientId);

        if (!playerData.HasValue) return;

        networkLobbyPlayers.Add(new NetworkLobbyPlayerState(
            clientId,
            playerData.Value.PlayerName,
            false
        ));
    }

    [ServerRpc(RequireOwnership =false)]
    private void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < networkLobbyPlayers.Count; i++)
        {
            if (networkLobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId)
            {
                networkLobbyPlayers[i] = new NetworkLobbyPlayerState(
                    networkLobbyPlayers[i].ClientId,
                    networkLobbyPlayers[i].PlayerName,
                    !networkLobbyPlayers[i].IsReady
                );
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) return;

        if (!IsEveryoneReady()) return;

        ServerGameNetworkPortal.Instance.StartGame();
    }

    public void OnLeaveClick()
    {
        GameNetworkPortal.Instance.RequestDisconnect();
    }

    public void OnReadyClick()
    {
        ToggleReadyServerRpc();
    }

    public void OnStartClick()
    {
        if (!IsServer) return;

        StartGameServerRpc();
    }

    private void HandleNetworkLobbyPlayerStateChanged(NetworkListEvent<NetworkLobbyPlayerState> networkLobbyPlayerState)
    {
        for (int i = 0; i < lobbyPlayerCards.Length; i++)
        {
            if (networkLobbyPlayers.Count > i)
            {
                lobbyPlayerCards[i].UpdateDisplay(networkLobbyPlayers[i]);
            }
            else
            {
                lobbyPlayerCards[i].DisableDisplay();
            }
        }

        if (IsHost)
        {
            startGameButton.interactable = IsEveryoneReady();
        }
    }
}
