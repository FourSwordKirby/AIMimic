using UnityEngine;
using System.Collections;

public class AirAttackState : State<Player>
{
    private Player player;
    private GameObject meleeHitbox;

    private float attackDistance;

    public float startup;
    public float duration;
    public float endlag;

    private float timer;

    private Vector3 startPosition;
    private Vector3 endPosition;

    public AirAttackState(Player playerInstance, StateMachine<Player> fsm) : base(playerInstance, fsm)
    {
        GameManager.instance.playSound("AirSwipe");

        player = playerInstance;
        meleeHitbox = player.hitboxManager.getHitbox("AirMeleeHitbox").gameObject;

        attackDistance = 0.55f;

        startup = 0.05f;
        duration = 0.05f;
        endlag = 1.2f;

        timer = 0;
    }

    override public void Enter()
    {
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white;
        meleeHitbox.GetComponent<SpriteRenderer>().flipX = player.sprite.flipX;

        startPosition = Vector3.zero;
        endPosition = player.facingDirection * attackDistance;

        if (player.comboCount >= 2)
        {
            player.chainable = false;
            meleeHitbox.GetComponent<Hitbox>().knockdown = true;
        }

        player.selfBody.angularVelocity = -540.0f * player.facingDirection.x;
    }

    override public void Execute()
    {
        //ANIMATE THE HITBOX MOVING
        timer += Time.deltaTime;
        if (timer < startup)
        {
            meleeHitbox.transform.localPosition = Vector3.Lerp(startPosition, endPosition, timer / startup);
        }
        else if (timer < startup + duration)
        {
            if(timer - Time.deltaTime < startup)
                player.hitboxManager.activateHitBox("AirMeleeHitbox");

            meleeHitbox.transform.localPosition = endPosition;
        }
        else if (timer < startup + duration + endlag)
        {
            if (timer - Time.deltaTime < startup + duration)
            {
                player.hitboxManager.deactivateHitBox("AirMeleeHitbox");
                meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
            }

            meleeHitbox.transform.localPosition = Vector3.Lerp(endPosition, startPosition, (timer - startup - duration) / endlag);
        }
        else
        {
            meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
            meleeHitbox.transform.localPosition = Vector2.zero;
        }
    }

    override public void FixedExecute()
    {
        //Hitting the ground early
        if (player.grounded && player.selfBody.velocity.y <= 0)
        {
            player.Stand();
            return;
        }
    }

    override public void Exit()
    {
        player.hitboxManager.deactivateHitBox("AirMeleeHitbox");
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
        meleeHitbox.transform.localPosition = Vector2.zero;
        player.selfBody.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        player.selfBody.angularVelocity = 0;
    }
}
