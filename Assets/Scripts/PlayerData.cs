using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerData
{
    public string PlayerName { get; private set; }
    public ulong ClientId { get; private set; }

    public PlayerData(string playerName, ulong clientId)
    {
        PlayerName = playerName;
        ClientId = clientId;
    }
}
