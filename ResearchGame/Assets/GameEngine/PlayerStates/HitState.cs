using UnityEngine;
using System.Collections;

public class HitState : State<Player>
{

    private Player player;
    private float hitlag;
    private float hitstun;
    private Vector2 knockback;
    private bool knockedDown;

    private float hitGravityScale = 20.0f;

    private float timer = 0.0f;
    private float animTime = 0.5f;

    public HitState(Player playerInstance, float hitlag, float hitstun, Vector2 knockback, bool knockedDown, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
        player.knockedDown = player.knockedDown || knockedDown;

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
    }

    override public void Execute()
    {
        if (timer < hitlag)
        {
            timer += Time.deltaTime;
            player.selfBody.isKinematic = true;
            
            if (timer >= hitlag)
            {
                knockback = new Vector2(-player.facingDirection.x * knockback.x, knockback.y);
                player.selfBody.isKinematic = false;
                player.selfBody.velocity = knockback;
            }
            return;
        }

        if (timer < hitlag + hitstun)
        {
            timer += Time.deltaTime;
            if(player.knockedDown)
            {
                player.StandAnim();
                player.spriteContainer.transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(0, Vector3.forward), Quaternion.AngleAxis(90.0f * player.facingDirection.x, Vector3.forward), (timer - hitlag) / animTime);

                if (timer >= hitlag + hitstun)
                {
                    GameManager.EndCombo(player.opponent);
                    player.ActionFsm.ChangeState(new TechState(player, player.ActionFsm));
                }
            }
            else
            {
                if (timer >= hitlag + hitstun)
                {
                    GameManager.EndCombo(player.opponent);
                    player.stunned = false;
                    player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
                }
            }
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

