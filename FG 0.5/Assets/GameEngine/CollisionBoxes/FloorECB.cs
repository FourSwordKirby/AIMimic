using UnityEngine;
using System.Collections;

public class FloorECB : MonoBehaviour {

    public Player player;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.gameObject.name == "Floor" && player.selfBody.velocity.y < 1.0f)
            player.grounded = true;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if(col.collider.gameObject.name == "Floor" && player.selfBody.velocity.y < 1.0f)
            player.grounded = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.gameObject.name == "Floor" && player.selfBody.velocity.y >= 1.0f)
            player.grounded = false;
    }
}
