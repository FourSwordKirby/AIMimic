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
    public float animDuration;

    private float frameCounter;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private float direction;

    public AirAttackState(Player playerInstance, StateMachine<Player> fsm) : base(playerInstance, fsm)
    {
        GameManager.instance.playSound("AirSwipe");

        player = playerInstance;
        meleeHitbox = player.hitboxManager.getHitbox("AirMeleeHitbox").gameObject;

        attackDistance = 0.55f;

        startup = 0.05f * Application.targetFrameRate;
        duration = 0.05f * Application.targetFrameRate;
        endlag = 1.2f * Application.targetFrameRate;

        animDuration = (int) (0.33333f * Application.targetFrameRate);
        frameCounter = 0;
    }

    override public void Enter()
    {
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white;
        meleeHitbox.GetComponent<SpriteRenderer>().flipX = player.sprite.flipX;

        startPosition = Vector3.zero;
        endPosition = player.facingDirection * attackDistance;
        direction = player.facingDirection.x;

        if (player.comboCount >= 2)
        {
            player.chainable = false;
            meleeHitbox.GetComponent<Hitbox>().knockdown = true;
        }
    }

    override public void Execute()
    {
        //ANIMATE THE HITBOX MOVING
        frameCounter++;
        if (frameCounter < startup)
        {
            meleeHitbox.transform.localPosition = Vector3.Lerp(startPosition, endPosition, frameCounter / startup);
        }
        else if (frameCounter < startup + duration)
        {
            if (frameCounter - Time.fixedDeltaTime < startup)
                player.hitboxManager.activateHitBox("AirMeleeHitbox");

            meleeHitbox.transform.localPosition = endPosition;
        }
        else if (frameCounter < startup + duration + endlag)
        {
            if (frameCounter - Time.fixedDeltaTime < startup + duration)
            {
                player.hitboxManager.deactivateHitBox("AirMeleeHitbox");
                meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
            }

            meleeHitbox.transform.localPosition = Vector3.Lerp(endPosition, startPosition, (frameCounter - startup - duration) / endlag);
        }
        else
        {
            meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
            meleeHitbox.transform.localPosition = Vector2.zero;
        }


        //Animate the player
        if (frameCounter < animDuration)
        {
            player.transform.rotation =
            Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(-direction * Vector3.forward * 179), frameCounter / animDuration);
        }
        else
        {
            player.transform.rotation =
            Quaternion.Lerp(Quaternion.Euler(-direction * Vector3.forward * 181), Quaternion.Euler(Vector3.zero), (frameCounter - animDuration) / animDuration);
        }

        //Hitting the ground early
        if (player.grounded && player.selfBody.velocity.y <= 0)
        {
            Parameters.InputDirection dir = Controls.getInputDirection(player);

            if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
                player.PerformAction(Action.Crouch);
            else
                player.PerformAction(Action.Stand);
            return;
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        player.hitboxManager.deactivateHitBox("AirMeleeHitbox");

        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
        meleeHitbox.transform.localPosition = Vector2.zero;
        meleeHitbox.GetComponent<Hitbox>().knockdown = false;

        player.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        player.selfBody.angularVelocity = 0;
    }
}
