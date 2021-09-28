using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneManager : MonoBehaviour
{
    public void OnQuitClick()
    {
        SceneManager.LoadScene("Lobby");
    }
}
