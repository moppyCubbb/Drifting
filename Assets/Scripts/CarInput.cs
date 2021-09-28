using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInput : MonoBehaviour
{

    CarMovement carMovement;

    // Start is called before the first frame update
    void Start()
    {
        carMovement = GetComponent<CarMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 inputVector = Vector2.zero;

        inputVector.x = Input.GetAxis("Horizontal");
        inputVector.y = Input.GetAxis("Vertical");

        carMovement.SetInputVector(inputVector);
    }
}
