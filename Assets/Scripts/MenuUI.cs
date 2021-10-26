using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{ 
    [SerializeField]
    private GameObject canvas;

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

    public void OnStartNetworkTestClick()
    {
        SceneManager.LoadScene("NetworkTestScene");
    }

    public void OnStartNetworkClick()
    {
        SceneManager.LoadScene("NetworkMenuScene");
    }
}
