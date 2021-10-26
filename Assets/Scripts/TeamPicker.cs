using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Connection;

public class TeamPicker : MonoBehaviour
{
    public void SelectTeam(int teamIndex)
    {
        //Debug.Log("select team " + teamIndex);
        // get local id
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(localClientId, out NetworkClient networkClient))
        {
            return;
        }

        if (!networkClient.PlayerObject.TryGetComponent<TeamPlayer>(out TeamPlayer teamPlayer))
        {
            return;
        }

        // send a message to server to set the local client's team
        teamPlayer.SetTeamServerRpc((byte)teamIndex);
    }
}
