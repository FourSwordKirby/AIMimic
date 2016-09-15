using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour {

    /// <summary>
    /// This is how long hte shield can be held up at full size
    /// </summary>
    public float shieldSize;
    public float currentShieldSize;

    private bool raised;

    /*self references*/
    private Vector3 initialScale;
    public SpriteRenderer sprite;
    public ShieldHurtbox hurtbox;

    void Start()
    {
        initialScale = this.transform.localScale;
        currentShieldSize = shieldSize;
        raised = false;
    }

	// Update is called once per frame
	void Update () {
        if (raised && currentShieldSize > 0)
        {
            currentShieldSize -= Time.deltaTime;
            this.transform.localScale = currentShieldSize/shieldSize * initialScale;
        }

        if (currentShieldSize < shieldSize && !raised)
        {
            currentShieldSize += Time.deltaTime;
        }
	}

    public void RaiseShield()
    {
        raised = true;
        sprite.enabled = true;
        hurtbox.gameObject.SetActive(true);
    }

    public void LowerShield()
    {
        raised = false;
        sprite.enabled = false;
        hurtbox.gameObject.SetActive(false);
    }
}
