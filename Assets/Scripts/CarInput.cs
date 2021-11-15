using UnityEngine;
using MLAPI;

public class CarInput : NetworkBehaviour
{

    CarMovement carMovement;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager.Singleton != null && !IsOwner) return;
        carMovement = GetComponent<CarMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Singleton != null && !IsOwner) return;
        Vector2 inputVector = Vector2.zero;

        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.y = Input.GetAxis("Vertical");

        carMovement.SetInputVector(inputVector);
    }
}
