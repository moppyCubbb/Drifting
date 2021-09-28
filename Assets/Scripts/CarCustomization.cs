using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarCustomization : MonoBehaviour
{
    public Slider maxSpeedSlider;
    public Slider accelerationSlider;
    public Slider steeringSlider;
    public Slider driftingSlider;

    public CarMovement[] carMovements;

    public void OnMaxSpeedChange()
    {
        foreach (CarMovement carMovement in carMovements)
        {
            carMovement.maximumSpeed = maxSpeedSlider.value;
        }
    }

    public void OnAccelerationChange()
    {
        foreach (CarMovement carMovement in carMovements)
        {
            carMovement.accelerationFactor = accelerationSlider.value;
        }
    }

    public void OnSteeringChange()
    {
        foreach (CarMovement carMovement in carMovements)
        {
            carMovement.steerFactor = steeringSlider.value;
        }
    }

    public void OnDriftingChange()
    {
        foreach (CarMovement carMovement in carMovements)
        {
            carMovement.driftFactor = driftingSlider.value;
        }
    }
}
