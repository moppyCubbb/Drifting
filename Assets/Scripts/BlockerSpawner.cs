using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class BlockerSpawner : NetworkBehaviour
{
    [SerializeField]
    private NetworkObject blockerPrefab;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 position = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0;
        SpawnBlockerServerRpc(position);
        /*Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);*/
        /*if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Interact")))
        {
            Debug.Log(hit.transform.name);
            SpawnBlockerServerRpc(hit.point);
        }*/
    }

    [ServerRpc]
    private void SpawnBlockerServerRpc(Vector3 spawnPosition)
    {
        NetworkObject blockerInstance = Instantiate(blockerPrefab, spawnPosition, Quaternion.identity);
        blockerInstance.SpawnWithOwnership(OwnerClientId);
    }
}
