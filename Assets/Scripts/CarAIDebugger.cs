using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAIDebugger : MonoBehaviour
{
    CarAIManager aiHandler;

    private void Start()
    {
        aiHandler = GetComponent<CarAIManager>();
    }

    
}
