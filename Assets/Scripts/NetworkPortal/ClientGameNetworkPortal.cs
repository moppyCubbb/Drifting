using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using MLAPI;

[RequireComponent(typeof(GameNetworkPortal))]
public class ClientGameNetworkPortal : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    NetworkObject playerPrefab;

    public static ClientGameNetworkPortal Instance => instance;
    private static ClientGameNetworkPortal instance;

    public DisconnectReason DisconnectReason { get; private set; } = new DisconnectReason();

    public event Action<ConnectionStatus> OnConnectionFinished;
    public event Action OnNetworkTimedOut;

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
        gameNetworkPortal.OnConnectionFinished += HandleConnectionFinished;
        gameNetworkPortal.OnDisconnectReasonReceived += HandleDisconnectReasonReceived;

        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientDisconnect;
    }

    private void OnDestroy()
    {
        if (gameNetworkPortal == null) return;

        gameNetworkPortal.OnNetworkReadied -= HandleNetworkReadied;
        gameNetworkPortal.OnConnectionFinished -= HandleConnectionFinished;
        gameNetworkPortal.OnDisconnectReasonReceived -= HandleDisconnectReasonReceived;

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientDisconnect;
    }

    public void StartClient()
    {
        var payload = JsonUtility.ToJson(new ConnectionPayload()
        {
            clientGuid = Guid.NewGuid().ToString(),
            clientScene = SceneManager.GetActiveScene().buildIndex,
            playerName = PlayerPrefs.GetString("PlayerName", "Missing Name")
        });

        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        NetworkManager.Singleton.StartClient();
    }

    private void HandleNetworkReadied()
    {
        if (!NetworkManager.Singleton.IsClient) return;
        if (!NetworkManager.Singleton.IsHost)
        {
            gameNetworkPortal.OnUserDisconnectRequested += HandleUserDisconnectRequested;
        }

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameNetworkPortal.ClientToServerSceneChanged(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandleUserDisconnectRequested()
    {
        DisconnectReason.SetDisconnectReason(ConnectionStatus.UserRequestDisconnect);
        NetworkManager.Singleton.StopClient();

        HandleClientDisconnect(NetworkManager.Singleton.LocalClientId);

        SceneManager.LoadScene("NetworkMenuScene");
    }

    private void HandleConnectionFinished(ConnectionStatus status)
    {
        if (status != ConnectionStatus.Success)
        {
            DisconnectReason.SetDisconnectReason(status);
        }

        OnConnectionFinished?.Invoke(status);
    }

    private void HandleDisconnectReasonReceived(ConnectionStatus status)
    {
        DisconnectReason.SetDisconnectReason(status);
    }

    private void HandleClientDisconnect(ulong obj)
    {
        if (!NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsHost)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            gameNetworkPortal.OnUserDisconnectRequested -= HandleUserDisconnectRequested;

            if (SceneManager.GetActiveScene().name != "NetworkMenuScene")
            {
                if (!DisconnectReason.HasTransitionReason)
                {
                    DisconnectReason.SetDisconnectReason(ConnectionStatus.GenericDisconnect);
                }
                SceneManager.LoadScene("NetworkMenuScene");
            }
            else
            {
                OnNetworkTimedOut?.Invoke();
            }
        }
    }
}
