using UnityEngine;
using System.Collections;

public class MeterOrb : MonoBehaviour {

    public int meterGain;

    public float respawnTime;
    private float respawnTimer;

    /*self references*/
    public SpriteRenderer sprite;
    public Collider2D pickupBox;

    void Update()
    {
        if (respawnTimer > 0)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                this.gameObject.SetActive(true);
                sprite.enabled = true;
                pickupBox.enabled = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        GameObject enterObject = col.gameObject;

        PlayerOriginPoint playerOriginPoint = enterObject.GetComponent<PlayerOriginPoint>();

        if (playerOriginPoint != null)
        {
            playerOriginPoint.player.gainMeter(meterGain);
            respawnTimer = respawnTime;

            sprite.enabled = false;
            pickupBox.enabled = false;
        }
    }
}
