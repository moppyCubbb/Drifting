using UnityEngine;
using UnityEngine.SceneManagement;
using MLAPI;

public class PlayerSceneManager : MonoBehaviour
{
    void Start()
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(NetworkManager.Singleton.LocalClientId,
            out var networkedClient))
        {
            var player = networkedClient.PlayerObject;
            if (player)
            {
                Debug.Log("player hei!");
            }
        }
    }
    public void OnQuitClick()
    {
        SceneManager.LoadScene("Lobby");
    }
}
