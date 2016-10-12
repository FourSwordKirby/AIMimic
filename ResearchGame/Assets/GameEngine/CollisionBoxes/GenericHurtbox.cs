using UnityEngine;
using System.Collections;

/*
 * The hurtbox does not respond when it's hit by something, it only provides ways for the hitbox to do different things to the hitboxes
 * The reason for this is because Hurtboxes have more consistent and adaptable behavior compared to other hitboxes
 */
public class GenericHurtbox : Hurtbox {
    override public void TakeDamage(float damage)
    {
        owner.LostHealth(damage);
    }

    override public void TakeHit(float hitlag, float hitstun, Vector2 knockback)
    {
        owner.ActionFsm.ChangeState(new HitState(owner, hitlag, hitstun, knockback, owner.ActionFsm));
        knockback = -Mathf.Sign(owner.facingDirection.x) * knockback;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Hitbox hitbox = col.GetComponent<Hitbox>();
        if(hitbox != null && hitbox.owner != this.owner)
        {
            if(owner.isBlocking)
            {
                Debug.Log("BLOCK");
                TakeDamage(hitbox.chipDamage);
                TakeHit(hitbox.hitlag, hitbox.blockstun, hitbox.knockbackVector);
            }
            else
            {
                TakeDamage(hitbox.damage);
                TakeHit(hitbox.hitlag, hitbox.hitstun, hitbox.knockbackVector);
            }
        }
    }
}
