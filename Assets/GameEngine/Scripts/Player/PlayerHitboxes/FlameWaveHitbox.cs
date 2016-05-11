using UnityEngine;
using System.Collections;

public class FlameWaveHitbox : Hitbox {
    /// <summary>
    /// The bool designates whether the super was started in the air or the ground
    /// </summary>
    public bool airborne;

    /// <summary>
    /// If the attack was started in the air, it is weaker than the grounded version.
    /// </summary>
    public float airborneMod;

    public float decayTime;

    void Update()
    {
        if (this.transform.parent.GetComponent<Rigidbody2D>().velocity.x < 0)
            this.transform.parent.GetComponent<SpriteRenderer>().flipX = true;

        decayTime -= Time.deltaTime;
        if (decayTime < 0)
        {
            Destroy(this.transform.parent.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Hurtbox hurtbox = col.gameObject.GetComponent<Hurtbox>();
        if (hurtbox != null && hurtbox.owner != this.owner)
        {
            float xDir = this.transform.parent.GetComponent<Rigidbody2D>().velocity.x;
            if (xDir == 0)
                xDir = 1;
            else
                xDir = xDir / Mathf.Abs(xDir);

            if (!airborne)
            {
                Vector2 appliedKnockbackVector = new Vector2(knockbackVector.x * xDir, knockbackVector.y);

                hurtbox.TakeDamage(damage);
                hurtbox.TakeHit(hitlag, hitstun, appliedKnockbackVector);
            }
            else
            {
                Vector2 appliedKnockbackVector = airborneMod * new Vector2(knockbackVector.x * xDir, knockbackVector.y);

                hurtbox.TakeDamage(airborneMod * damage);
                hurtbox.TakeHit(hitlag, hitstun, appliedKnockbackVector);
            }

            Destroy(this.transform.parent.gameObject);
        }
    }
}