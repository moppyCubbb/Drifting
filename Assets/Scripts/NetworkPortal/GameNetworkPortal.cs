using System;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using MLAPI.Transports;
using UnityEngine;

// shared network operations
public class GameNetworkPortal : MonoBehaviour
{
    public static GameNetworkPortal Instance => instance;
    private static GameNetworkPortal instance;

    public event Action OnNetworkReadied;
    public event Action<ConnectionStatus> OnConnectionFinished;
    public event Action<ConnectionStatus> OnDisconnectReasonReceived;
    public event Action<ulong, int> OnClientSceneChanged;
    public event Action OnUserDisconnectRequested;

    private string password;

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
        NetworkManager.Singleton.OnServerStarted += HandleNetworkReady;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;

        RegisterClientMessageHandlers();
        RegisterServerMessageHandlers();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= HandleNetworkReady;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;

            UnregisterClientMessageHandlers();
            UnregisterServerMessageHandlers();
        }
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost(null, null, false);
    }

    public void RequestDisconnect()
    {
        OnUserDisconnectRequested?.Invoke();
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        HandleNetworkReady();
    }

    private void HandleNetworkReady()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            OnConnectionFinished?.Invoke(ConnectionStatus.Success);
        }

        OnNetworkReadied?.Invoke();
    }

    #region message handlers
    private void RegisterServerMessageHandlers()
    {
        CustomMessagingManager.RegisterNamedMessageHandler("ClientToServerSceneChanged", (senderClientId, stream) =>
        {
            using (var reader = PooledNetworkReader.Get(stream))
            {
                int sceneIndex = reader.ReadInt32();

                OnClientSceneChanged?.Invoke(senderClientId, sceneIndex);
            }
        });
    }

    private void RegisterClientMessageHandlers()
    {
        CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientConnectResult", (senderClientId, stream) =>
        {
            using (var reader = PooledNetworkReader.Get(stream))
            {
                ConnectionStatus status = (ConnectionStatus)reader.ReadInt32();

                OnConnectionFinished?.Invoke(status);
            }
        });

        CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientSetDisconnectReason", (senderClientId, stream) =>
        {
            using (var reader = PooledNetworkReader.Get(stream))
            {
                ConnectionStatus status = (ConnectionStatus)reader.ReadInt32();

                OnDisconnectReasonReceived?.Invoke(status);
            }
        });
    }

    private void UnregisterServerMessageHandlers()
    {
        CustomMessagingManager.UnregisterNamedMessageHandler("ClientToServerSceneChanged");
    }

    private void UnregisterClientMessageHandlers()
    {
        CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientConnectResult");
        CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientSetDisconnectReason");
    }
    #endregion

    #region message senders
    public void ClientToServerSceneChanged(int newScene)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            OnClientSceneChanged?.Invoke(NetworkManager.Singleton.ServerClientId, newScene);
        }
        else if (NetworkManager.Singleton.IsConnectedClient)
        {
            using (var buffer = PooledNetworkBuffer.Get())
            {
                using (var writer = PooledNetworkWriter.Get(buffer))
                {
                    writer.WriteInt32(newScene);
                    CustomMessagingManager.SendNamedMessage("ClientToServerSceneChanged", NetworkManager.Singleton.ServerClientId, buffer, NetworkChannel.Internal);
                }
            }
        }
    }

    public void ServerToClientConnectResult(ulong netId, ConnectionStatus status)
    {
        using (var buffer = PooledNetworkBuffer.Get())
        {
            using (var writer = PooledNetworkWriter.Get(buffer))
            {
                writer.WriteInt32((int)status);
                CustomMessagingManager.SendNamedMessage("ServerToClientConnectResult", netId, buffer, NetworkChannel.Internal);
            }
        }
    }

    public void ServerToClientSetDisconnectReason(ulong netId, ConnectionStatus status)
    {
        using (var buffer = PooledNetworkBuffer.Get())
        {
            using (var writer = PooledNetworkWriter.Get(buffer))
            {
                writer.WriteInt32((int)status);
                CustomMessagingManager.SendNamedMessage("ServerToClientSetDisconnectReason", netId, buffer, NetworkChannel.Internal);
            }
        }
    }
    #endregion
}
