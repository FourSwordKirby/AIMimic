using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    private float targetHealth;
    private float currentHealth;
    private float maxHealth;

    private Image imageComponent;

    void Start()
    {
        imageComponent = this.GetComponent<Image>();
    }

	void Update () {
       if (currentHealth < targetHealth)
            currentHealth += 1;
       if (currentHealth > targetHealth)
            currentHealth -= 1;

        imageComponent.fillAmount = currentHealth / maxHealth;

	}

    public void SetHealth(float health)
    {
        this.targetHealth = health;
    }

    public void SetMaxHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
    }
}
