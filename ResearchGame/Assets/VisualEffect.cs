using UnityEngine;
using System.Collections;

public class VisualEffect : MonoBehaviour {

    public SpriteRenderer spriteRenderer;

    public float duration;
    private float timer;


	// Update is called once per frame
	void Update () {
	    if(timer < duration)
        {
            timer += Time.deltaTime;
            this.transform.localScale = Vector2.Lerp(Vector3.zero, Vector3.one, timer / duration);
            spriteRenderer.color = Color.Lerp(Color.white, Color.clear, (timer - duration / 2) / (duration / 2));
            if (timer >= duration)
                Destroy(this.gameObject);
        }
	}
}
