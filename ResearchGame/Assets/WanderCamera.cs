using UnityEngine;
using System.Collections;

public class WanderCamera : MonoBehaviour {

    float xdirection = 0.01f;
    float ydirection = 0.01f;

	// Update is called once per frame
	void Update () {
        Debug.Log("Hey");
        this.transform.position += Vector3.right * xdirection + Vector3.up * ydirection;
        if (this.transform.position.x > 5 || this.transform.position.x < -5)
            xdirection = Random.Range(0.005f, 0.015f) * -Mathf.Sign(xdirection);
        if (this.transform.position.y > 5 || this.transform.position.y < -5)
            ydirection = Random.Range(0.005f, 0.015f) * -Mathf.Sign(ydirection);
    }
}
