using UnityEngine;
using UnityEngine.UI;

public class SliderValueText : MonoBehaviour
{
    public Text displayText;

    Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
        displayText.text = slider.value.ToString();
    }

    public void OnSliderValueChange()
    {
        displayText.text = slider.value.ToString();
    }
}
