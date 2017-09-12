using UnityEngine;
using System.Collections;

public class PlayerECB : MonoBehaviour {
    public Player player;

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.GetComponent<PlayerECB>() != null)
        {
            Vector2 displacementVector = this.transform.position - col.transform.position;
            player.selfBody.velocity += displacementVector.normalized * 0.1f;
        }
    }
}
