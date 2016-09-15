using UnityEngine;
using System.Collections;

/*
 * The hurtbox does not respond when it's hit by something, it only provides ways for the hitbox to do different things to the hitboxes
 * The reason for this is because Hurtboxes have more consistent and adaptable behavior compared to other hitboxes
 */
public abstract class Hurtbox : Collisionbox {
    public Player owner;

    /// <summary>
    /// Deals damage to the target object
    /// </summary>
    public abstract void TakeDamage(float damage);

    /// <summary>
    /// This function will apply these main features to the hurtbox that is getting hit
    /// In the case that thereis no hitstun or hitlag, it is advised to not go into a hitstun state
    /// </summary>
    public abstract void TakeHit(float hitlag, float hitstun, Vector2 knockback);
}
