using UnityEngine;
using System.Collections;

public class HitState : State<Player>
{

    private Player player;
    private float hitlag;
    private float hitstun;
    private Vector2 knockback;
    private bool knockedDown;

    private float hitGravityScale = 25.0f;

    private float frameCounter = 0.0f;
    private float knockdownAnimFrameTime = 0.5f * Application.targetFrameRate;

    public HitState(Player playerInstance, float hitlag, float hitstun, Vector2 knockback, bool knockedDown, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;

        //probably need some means for denoting tumble etc.

        this.hitlag = hitlag;
        this.hitstun = hitstun;
        this.knockback = knockback;
        this.knockedDown = knockedDown;
    }

    override public void Enter()
    {
        player.sprite.sprite = player.hitSprite;
        player.selfBody.gravityScale = hitGravityScale;
        player.selfBody.mass = 1000;

        player.stunned = true;
        player.knockedDown = player.knockedDown || knockedDown;
    }

    override public void Execute()
    {
        if (frameCounter < hitlag)
        {
            frameCounter ++;
            player.selfBody.simulated = false;
            
            if (frameCounter >= hitlag)
            {
                knockback = new Vector2(-player.facingDirection.x * knockback.x, knockback.y);
                player.selfBody.simulated = true;
                player.selfBody.velocity = knockback;
            }
            return;
        }

        if (frameCounter < hitlag + hitstun)
        {
            frameCounter++;
            if (player.knockedDown)
            {
                player.StandAnim();
                player.spriteContainer.transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(0, Vector3.forward), Quaternion.AngleAxis(90.0f * player.facingDirection.x, Vector3.forward), (frameCounter - hitlag) / knockdownAnimFrameTime);
            }
            else
            {
                if (frameCounter >= hitlag + hitstun)
                {
                    GameManager.EndCombo(player.opponent);
                    player.ExitHitstun();
                }
            }
            return;
        }

        if(player.knockedDown)
        {
            GameManager.EndCombo(player.opponent);

            //Modify this to put the player in a teching state and to record how long they wait to tech etc.
            //We need to record how long a player waits to tech etc.

            Parameters.InputDirection dir = Controls.getInputDirection(player);
            if(dir == Parameters.InputDirection.W)
                player.Tech(-1.0f);
            if (dir == Parameters.InputDirection.E)
                player.Tech(1.0f);
            if(dir == Parameters.InputDirection.N)
                player.Tech(0.0f);
        }
    }

    override public void FixedExecute()
    {
        if(player.grounded)
        {
            float xNew = Mathf.Max(0.0f, Mathf.Abs(player.selfBody.velocity.x) - 0.1f);
            player.selfBody.velocity = new Vector2(Mathf.Sign(player.selfBody.velocity.x) * xNew, player.selfBody.velocity.y);
        }
    }

    override public void Exit()
    {
        player.sprite.sprite = player.normalSprite;
        player.selfBody.mass = 1;
    }
}

