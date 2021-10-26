using System;

[Serializable]
public class ConnectionPayload
{
    public string clientGuid;
    public int clientScene = -1;
    public string playerName;
    /*public bool usePassword = false;
    public string password;*/
}
