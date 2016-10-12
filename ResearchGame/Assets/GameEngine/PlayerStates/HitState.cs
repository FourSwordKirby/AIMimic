using UnityEngine;
using System.Collections;

public class HitState : State<Player>
{

    private Player player;
    private float hitlag;
    private float hitstun;
    private Vector2 knockback;

    public HitState(Player playerInstance, float hitlag, float hitstun, Vector2 knockback, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;

        //probably need some means for denoting tumble etc.

        this.hitlag = hitlag;
        this.hitstun = hitstun;
        this.knockback = knockback;
    }

    override public void Enter()
    {
    }

    override public void Execute()
    {
        if (hitlag > 0)
        {
            hitlag -= Time.deltaTime;
            player.selfBody.velocity = Vector2.zero;
            
            if (hitlag <= 0)
            {
                if (player.grounded)
                    knockback = new Vector2(knockback.x, 0);
                player.selfBody.velocity = knockback;
            }
            return;
        }

        if (hitstun > 0)
        {
            hitstun -= Time.deltaTime;

            if (hitstun <= 0)
            {
                player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
            }
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
    }
}

