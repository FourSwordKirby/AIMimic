using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MeterBar : MonoBehaviour {

    private float currentMeter;
    private float maxMeter;

    private Image imageComponent;

    void Start()
    {
        imageComponent = this.GetComponent<Image>();
    }

    void Update()
    {
        imageComponent.fillAmount = currentMeter/ maxMeter;
    }

    public void SetMeter(float meter)
    {
        this.currentMeter = meter;
    }

    public void SetMaxMeter(float maxMeter)
    {
        this.maxMeter = maxMeter;
    }
}
