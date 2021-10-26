using UnityEngine;
using TMPro;

public class NetworkMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TMP_InputField displayNameInputField;

    private void Start()
    {
        PlayerPrefs.GetString("PlayerName");
    }

    public void OnHostClick()
    {
        PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
        GameNetworkPortal.Instance.StartHost();
    }

    public void OnClientClick()
    {
        PlayerPrefs.SetString("PlayerName", displayNameInputField.text);
        ClientGameNetworkPortal.Instance.StartClient();
    }
}
