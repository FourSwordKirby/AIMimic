using UnityEngine;
using System.Collections;

public abstract class Collisionbox : MonoBehaviour {

    public string boxName;
    private Collider2D[] colliders;

    void Awake()
    {
        colliders = this.GetComponents<Collider2D>();
    }

    public void Activate()
    {
        foreach(Collider2D col in colliders)
        {
            col.enabled = true;
        }
    }

    public void Deactivate()
    {
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }
    }
}
