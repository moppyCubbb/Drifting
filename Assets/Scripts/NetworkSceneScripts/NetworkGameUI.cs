using UnityEngine;

public class NetworkGameUI : MonoBehaviour
{
    public void OnLeaveClick()
    {
        GameNetworkPortal.Instance.RequestDisconnect();
    }
}
