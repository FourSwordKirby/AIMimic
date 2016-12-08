using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Hitbox : Collisionbox{
    public Player owner;

    public float damage;
    public Vector2 knockbackVector;
    public float hitlag;
    public float hitstun;

    public float chipDamage;
    public float blockstun;
    public bool isProjectile;

    public hitType type;
    public bool knockdown;

    public int prioirty;
    public float meterGain;

    public enum hitType
    {
        mid,
        low,
        high
    }
}
