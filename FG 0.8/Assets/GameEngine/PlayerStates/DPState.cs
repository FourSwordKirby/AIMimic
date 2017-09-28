using UnityEngine;
using System.Collections;

public class DPState : State<Player>
{
    private Player player;
    private GameObject meleeHitbox;

    private float direction;

    public float startup;
    public float duration;
    public float endlag;

    public float invulnDuration;
    public float animDuration;

    private float frameCounter;


    public DPState(Player playerInstance, StateMachine<Player> fsm, int comboCount = 0) : base(playerInstance, fsm)
    {
        Debug.Log("Buff DP and make its input the Right Trigger");

        player = playerInstance;
        player.comboCount = comboCount;

        direction = player.facingDirection.x;

        //Change this to DP hitbox or something
        meleeHitbox = player.hitboxManager.getHitbox("DPHitbox").gameObject;

        startup = 0.10f * Application.targetFrameRate;
        duration = 0.10f * Application.targetFrameRate;
        endlag = 0.75f * Application.targetFrameRate;

        invulnDuration = 0.25f * Application.targetFrameRate;
        animDuration = (int)(0.33333f * Application.targetFrameRate);

        frameCounter = 0;
    }

    override public void Enter()
    {
        GameManager.instance.PlaySound("AirSwipe");
        meleeHitbox.transform.localEulerAngles = new Vector3(meleeHitbox.transform.rotation.x,
                                                        meleeHitbox.transform.rotation.y,
                                                        (direction == -1) ? -180.0f + 41.035f : -41.035f);
        meleeHitbox.transform.localScale = new Vector3(meleeHitbox.transform.localScale.x,
                                                        -direction * Mathf.Abs(meleeHitbox.transform.localScale.y),
                                                        meleeHitbox.transform.localScale.z);

        //Keeping track of player status
        player.status = PlayerStatus.DP;

        player.locked = true;
    }

    public Vector2 getJumpVelocity(float height, float distance, float time)
    {
        float xVel = distance / time;
        float yVel = 2 * height / (0.5f * time);
        player.selfBody.gravityScale = -yVel / ((0.5f * time) * Physics2D.gravity.y);

        //x = vt + 1/2(a)t^2
        //a = -v/t

        //x = vt - 1/2(v)t
        //x = 1/2 vt
        return new Vector2(xVel, yVel);
    }

    override public void Execute()
    {
        //Keeping track of player status
        if (frameCounter < startup + duration)
            player.status = PlayerStatus.DP;
        else
            player.status = PlayerStatus.Recovery;

        frameCounter++;

        //ANIMATIONS
        //Color
        if (frameCounter < startup + duration)
            meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white;
        else
            meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;

        //Hitbox management
        if (frameCounter < startup)
        {
            player.hitboxManager.deactivateHitBox("DPHitbox");
        }
        else if (frameCounter < startup + duration)
        {
            if (frameCounter - Time.deltaTime < startup)
            {
                player.hitboxManager.activateHitBox("DPHitbox");
                this.player.selfBody.velocity = getJumpVelocity(player.directionJumpHeight * 0.75f, direction * 0.5f, 0.7f);
            }
            player.grounded = false;
        }
        else if (frameCounter < startup + duration + endlag)
        {
            if (frameCounter - Time.fixedDeltaTime < startup + duration)
                player.hitboxManager.deactivateHitBox("DPHitbox");
        }

        //Controling invulnerability
        if (frameCounter < invulnDuration)
            player.StartInvuln();
        else
            player.EndInvuln();

        //Animate the player
        if (frameCounter < animDuration)
        {
            player.transform.rotation =
            Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(-direction * Vector3.forward * 181), frameCounter / animDuration);
        }
        else
        {
            player.transform.rotation =
            Quaternion.Lerp(Quaternion.Euler(-direction * Vector3.forward * 179), Quaternion.Euler(Vector3.zero), (frameCounter - animDuration) / animDuration);
        }

        //Handles the animation if the sprite is coming out of a knocked down state
        //TODO adjust flash kick damage and frame data
        if(player.spriteContainer.transform.localRotation != Quaternion.AngleAxis(0, Vector3.forward))
            player.spriteContainer.transform.localRotation = Quaternion.Lerp(Quaternion.AngleAxis(90.0f * player.facingDirection.x, Vector3.forward), Quaternion.AngleAxis(0, Vector3.forward), frameCounter / animDuration);

        //Handles for hitting the ground
        if (player.grounded && player.selfBody.velocity.y <= 0)
        {
            if (frameCounter > startup + duration)
                player.selfBody.velocity = Vector2.zero;

            if (frameCounter > startup + duration + endlag)
            {
                player.locked = false;
                //Handles doing a DP after a knockdown
                if (player.knockedDown)
                {
                    player.knockedDown = false;
                    player.ExitHitstun();
                }

                Parameters.InputDirection dir = Controls.getInputDirection(player);

                if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
                    player.PerformAction(Action.Crouch);
                else
                    player.PerformAction(Action.Stand);
                return;
            }
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        player.EndInvuln();

        this.player.selfBody.drag = 0.0f;
        player.chainable = false;

        player.hitboxManager.deactivateHitBox("MeleeHitbox");
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
    }

    override public State<Player> Copy()
    {
        DPState attackCopy = new DPState(this.Owner, this.Owner.ActionFsm, player.comboCount);
        attackCopy.frameCounter = frameCounter;

        attackCopy.player.chainable = player.chainable;
        attackCopy.meleeHitbox.GetComponent<Hitbox>().knockdown = meleeHitbox.GetComponent<Hitbox>().knockdown;
        attackCopy.meleeHitbox.GetComponent<Hitbox>().type = meleeHitbox.GetComponent<Hitbox>().type;
        attackCopy.meleeHitbox.transform.localPosition = meleeHitbox.transform.localPosition;
        return attackCopy;
    }
}
