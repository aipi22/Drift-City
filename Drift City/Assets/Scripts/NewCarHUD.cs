using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewCarHUD : MonoBehaviour
{
    public Rigidbody rb;

    public TMP_Text speedometer;
    public TMP_Text rpmMeter;
    public TMP_Text gear;

    public Slider throttleSlider;
    public Slider brakeSlider;

    public CarController controller;

    void Update()
    {
        if (controller != null)
        {
            float speed = rb.velocity.magnitude * 2.37f;

            speedometer.text = $"{speed:F0}";
            rpmMeter.text = $"{controller.RPM}";
            gear.text = $"{controller.currentGear}";

            if (throttleSlider != null)
            {
                float currentThrottle = controller.throttleInput;
                throttleSlider.value = currentThrottle;
            }
            if (brakeSlider != null)
            {
                bool currentBrake = controller.isBraking;
                if (currentBrake == true)
                {
                    brakeSlider.value = 1;
                }
                else
                {
                    brakeSlider.value = 0;
                }
            }
        }
    }
}
