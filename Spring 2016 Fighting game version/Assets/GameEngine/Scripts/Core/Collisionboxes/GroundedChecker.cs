using UnityEngine;
using System.Collections;

public class GroundedChecker : MonoBehaviour {
    public Player owner;

    public LayerMask floorMask;

    void Update()
    {
        if (owner.selfBody.velocity.y <= 0)
        {
            //Note, might just make it raycast when the player isn't grounded
            Debug.DrawRay(this.transform.position, 0.3f * Vector3.down, Color.red);

            if (Physics2D.Raycast(this.transform.position, Vector3.down, 0.3f, floorMask))
            {
                owner.grounded = true;
            }
            else
            {
                owner.grounded = false;
            }
        }
    }
}
