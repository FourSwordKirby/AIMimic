using UnityEngine;
using System.Collections;

public class FloorTile : MonoBehaviour {

    public SpriteRenderer spriteRenderer ;
    public bool triggered;

    public float activateTime;
    private float activateTimer;

    public float resetTime;
    private float resetTimer;

    public Color activeColor;
    public Color inactiveColor;

    public FloorOrganizer parentOrganizer;
    public int xPos;
    public int yPos;

	// Use this for initialization
	void Awake () {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        spriteRenderer.color = inactiveColor;
	}

    void Update()
    {
        if(triggered)
        {
            activateTimer += Time.deltaTime;
            Color newColor = Color.Lerp(inactiveColor, activeColor, (activateTimer) / activateTime);
            spriteRenderer.color = newColor;
            if (spriteRenderer.color == activeColor)
            {
                triggered = false;
                activateTimer = 0;
            }
        }
        else if(spriteRenderer.color != inactiveColor)
        {
            resetTimer += Time.deltaTime;
            Color newColor = Color.Lerp(activeColor, inactiveColor, (resetTimer) / resetTime);
            spriteRenderer.color = newColor;
            if (spriteRenderer.color == inactiveColor)
            {
                resetTimer = 0;
            }
        }
    }

    public void toggle() {
        spriteRenderer.color = inactiveColor;
        triggered = true;
    }
}
