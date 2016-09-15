using UnityEngine;
using System.Collections;

public abstract class Collisionbox : MonoBehaviour {

    public string name;
    private Collider2D[] colliders;

    void Awake()
    {
        colliders = this.GetComponents<Collider2D>();
    }

    public void Activate()
    {
        this.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        this.gameObject.SetActive(false);
    }
}
