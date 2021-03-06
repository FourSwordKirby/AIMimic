﻿using UnityEngine;
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

    override public void TakeHit(float hitlag, float hitstun, Vector2 knockback, bool knockdown)
    {
        if (knockdown)
            GameManager.instance.PlaySound("ComboEnder");
        else
            GameManager.instance.PlaySound("PunchHit");

        owner.EnterHitstun(hitlag, hitstun, knockback, knockdown);
    }

    override public void BlockHit(float hitlag, float hitstun, Vector2 knockback, bool knockdown)
    {
        GameManager.instance.PlaySound("Block");
        owner.selfBody.velocity = -owner.facingDirection.x* knockback;
        owner.ActionFsm.SuspendState(new HitlagState(owner, owner.ActionFsm, hitstun, owner.ActionFsm.CurrentState));
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Hitbox hitbox = col.GetComponent<Hitbox>();
        if(hitbox != null && hitbox.owner != this.owner)
        {
            hitbox.Deactivate();

            Vector3 hitLocation = (this.transform.position + col.bounds.ClosestPoint(this.transform.position))/2.0f;

            if (owner.isBlocking && !((hitbox.type == Hitbox.hitType.low && !owner.isCrouching) || (hitbox.type == Hitbox.hitType.high && owner.isCrouching)))
            {
                hitbox.owner.selfBody.velocity -= 1.5f * hitbox.owner.facingDirection.x * Vector2.right;
                //COMMENTED OUT FOR RESEARCH hitbox.owner.chainable = true;
                hitbox.owner.ActionFsm.SuspendState(new HitlagState(hitbox.owner, hitbox.owner.ActionFsm, hitbox.hitlag, hitbox.owner.ActionFsm.CurrentState));

                TakeDamage(hitbox.chipDamage);
                BlockHit(hitbox.hitlag, hitbox.blockstun + hitbox.hitlag, 1.0f * Vector2.right * hitbox.knockbackVector.x, false);
                
                GameManager.SpawnBlockIndicator(hitLocation);
                EventManager.instance.RecordBlock(hitbox.owner, owner, hitbox);
            }
            else
            {
                hitbox.owner.selfBody.velocity -= 0.25f * hitbox.owner.facingDirection.x * Vector2.right;
                //COMMENTED OUT FOR RESEARCH hitbox.owner.chainable = true;
                hitbox.owner.ActionFsm.SuspendState(new HitlagState(hitbox.owner, hitbox.owner.ActionFsm, hitbox.hitlag, hitbox.owner.ActionFsm.CurrentState));
                
                TakeDamage(hitbox.damage);
                if(owner.grounded)
                {
                    if (!hitbox.knockdown && !owner.knockedDown)
                        TakeHit(hitbox.hitlag, hitbox.hitstun, Vector2.right * hitbox.knockbackVector.x, hitbox.knockdown); //Keeps the player grounded
                    else
                        TakeHit(hitbox.hitlag, hitbox.hitstun, hitbox.knockbackVector, hitbox.knockdown);
                }
                else
                    TakeHit(hitbox.hitlag, hitbox.hitstun, hitbox.knockbackVector, true);
                
                GameManager.AddCombo(hitbox.owner);
                GameManager.SpawnHitIndicator(hitLocation);

                EventManager.instance.RecordHit(hitbox.owner, owner, hitbox, hitbox.owner.comboCount);
            }
        }
    }
}
