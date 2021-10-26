using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;

public class TeamPlayer : NetworkBehaviour
{
    [SerializeField]
    private Renderer teamColorRenderer;

    [SerializeField]
    private Color[] teamColors;

    private NetworkVariableByte teamIndex = new NetworkVariableByte(4);
    // default setting: only server can write, everyone can read

    [ServerRpc]
    // method called on the client and executed on the server side
    public void SetTeamServerRpc(byte newTeamIndex)
    {
        // return if the new team index is not valid
        if (newTeamIndex >= teamColors.Length) return;

        teamIndex.Value = newTeamIndex;
    }

    private void OnEnable()
    {
        teamIndex.OnValueChanged += OnTeamChanged;
    }

    private void OnDisable()
    {
        teamIndex.OnValueChanged -= OnTeamChanged;
    }

    private void OnTeamChanged(byte teamIndex, byte newTeamIndex)
    {
        // only client need to update renderer
        if (!IsClient) return;

        SpriteRenderer spriteRenderer = (SpriteRenderer)teamColorRenderer;
        if (spriteRenderer)
        {
            bool isDefault = newTeamIndex == teamColors.Length;
            spriteRenderer.color = isDefault ? spriteRenderer.color : teamColors[newTeamIndex];
        }
        //teamColorRenderer.material.SetColor("", teamColors[newTeamIndex]);
    }
}
