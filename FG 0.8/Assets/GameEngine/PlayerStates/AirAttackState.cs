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
        player = playerInstance;
        meleeHitbox = player.hitboxManager.getHitbox("AirMeleeHitbox").gameObject;

        attackDistance = 0.5f;

        startup = 0.15f * Application.targetFrameRate;
        duration = 0.05f * Application.targetFrameRate;
        endlag = 1.1f * Application.targetFrameRate;

        animDuration = (int) (0.15f * Application.targetFrameRate);
        frameCounter = 0;
    }

    override public void Enter()
    {
        GameManager.instance.PlaySound("AirSwipe");

        startPosition = Vector3.zero + Vector3.up * 0.1f;
        endPosition = player.facingDirection * attackDistance + Vector3.up * 0.1f;
        direction = player.facingDirection.x;

        meleeHitbox.GetComponent<SpriteRenderer>().flipX = player.sprite.flipX;
        meleeHitbox.GetComponent<Collider2D>().offset = new Vector2(direction, meleeHitbox.GetComponent<Collider2D>().offset.y);


        if (player.comboCount >= 2)
        {
            player.chainable = false;
            meleeHitbox.GetComponent<Hitbox>().knockdown = true;
        }

        //Dumb size animations
        player.spriteContainer.transform.localScale = new Vector3(1.0f,  0.8f, 1.0f);

        //Keeping track of player status
        player.status = PlayerStatus.AirAttack;
    }

    override public void Execute()
    {   
        //Keeping track of player status
        if (frameCounter < startup + duration)
            player.status = PlayerStatus.AirAttack;
        else
            player.status = PlayerStatus.Recovery;

        frameCounter++;

        //ANIMATIONS
        //Color
        if (frameCounter < startup + animDuration/2)
            meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white;
        else if(frameCounter < startup + animDuration)
            meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white - Color.black/2;
        else
            meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;

        //Spinning animation
        if (frameCounter < startup)
        {
            meleeHitbox.transform.localPosition = Vector3.Lerp(startPosition, endPosition, frameCounter / startup);
        }
        else if (frameCounter < startup + duration)
        {
            if (frameCounter - Time.deltaTime < startup)
                player.hitboxManager.activateHitBox("AirMeleeHitbox");

            meleeHitbox.transform.localPosition = endPosition;
        }
        else if (frameCounter < startup + duration + endlag)
        {
            if (frameCounter - Time.deltaTime < startup + duration)
                player.hitboxManager.deactivateHitBox("AirMeleeHitbox");

            meleeHitbox.transform.localPosition = Vector3.Lerp(endPosition, startPosition, (frameCounter - startup - duration) / endlag);
        }
        else
            meleeHitbox.transform.localPosition = Vector2.zero;
        

        //Animate the player
        if(frameCounter < startup * 0.75f)
        {
            player.transform.rotation =
            Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(direction * Vector3.forward * 15), frameCounter / (startup * 0.75f));
        }
        else if (frameCounter < startup)
        {
            player.transform.rotation =
            Quaternion.Lerp(Quaternion.Euler(direction * Vector3.forward * 15), Quaternion.Euler(Vector3.zero), frameCounter / (startup * 0.25f));
        }
        else if (frameCounter < startup + animDuration)
        {
            player.transform.rotation =
            Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(-direction * Vector3.forward * 179), (frameCounter-startup) / animDuration);
        }
        else
        {
            player.transform.rotation =
            Quaternion.Lerp(Quaternion.Euler(-direction * Vector3.forward * 181), Quaternion.Euler(Vector3.zero), (frameCounter - startup) / animDuration);
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

        //Dumb size animations
        player.spriteContainer.transform.localScale = Vector3.one;
    }

    override public State<Player> Copy()
    {
        AirAttackState attackCopy = new AirAttackState(this.Owner, this.Owner.ActionFsm);
        attackCopy.frameCounter = frameCounter;

        attackCopy.meleeHitbox.GetComponent<SpriteRenderer>().flipX = meleeHitbox.GetComponent<SpriteRenderer>().flipX;
        attackCopy.startPosition = startPosition;
        attackCopy.endPosition = endPosition;
        attackCopy.direction = direction;
        return attackCopy;
    }
}
