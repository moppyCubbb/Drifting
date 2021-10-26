using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Blocker : NetworkBehaviour
{
    [SerializeField]
    private Renderer blockerRenderer;

    private NetworkVariableColor blockerColor = new NetworkVariableColor();
    private NetworkVariableFloat blockerScale = new NetworkVariableFloat();

    public override void NetworkStart()
    {
        if (!IsServer) return;

        // Create random color for the blocker
        blockerColor.Value = Random.ColorHSV();
        blockerScale.Value = Random.Range(0.5f, 1.5f);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!Input.GetMouseButtonDown(1)) return;

        DestroyBlockerServerRpc();
    }

    private void OnEnable()
    {
        blockerColor.OnValueChanged += OnBlockerColorChanged;
        blockerScale.OnValueChanged += OnBlockerScaleChanged;
    }

    private void OnDisable()
    {
        blockerColor.OnValueChanged -= OnBlockerColorChanged;
        blockerScale.OnValueChanged -= OnBlockerScaleChanged;
    }

    private void OnBlockerColorChanged(Color oldColor, Color newColor)
    {
        if (!IsClient) return;

        SpriteRenderer renderer = (SpriteRenderer)blockerRenderer;
        if (renderer)
        {
            renderer.color = newColor;
        }
    }

    private void OnBlockerScaleChanged(float oldScale, float newScale)
    {
        if (!IsClient) return;

        transform.localScale = new Vector3(newScale, newScale, 1);
    }

    [ServerRpc]
    public void DestroyBlockerServerRpc()
    {
        // this would destroy the object on clients but still stays on server
        // GetComponent<NetworkObject>().Despawn();

        // by destroying a networkobject on the server, 
        // the object will then be destroyed on all clients
        Destroy(gameObject);
    }
}
