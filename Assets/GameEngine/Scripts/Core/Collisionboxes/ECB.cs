using UnityEngine;
using System.Collections;

/// <summary>
/// ECB is short for environmental collision box. This is used to check for collisions with floors, walls etc.
/// </summary>
public class ECB : Collisionbox {

    public void fallThrough()
    {
        if(this.gameObject.layer == LayerMask.NameToLayer("ECB"))
            this.gameObject.layer = LayerMask.NameToLayer("Fall Through");
    }

    void OnCollisionExit2D()
    {
        this.gameObject.layer = LayerMask.NameToLayer("ECB");
    }
}
