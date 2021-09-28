using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public void OnStartPlayerClick()
    {
        SceneManager.LoadScene("PlayerScene");
    }
    public void OnStartAIClick()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}
