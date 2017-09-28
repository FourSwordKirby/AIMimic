using UnityEngine;
using System.Collections;

public class DashState : State<Player>
{
    private Player player;

    private bool airborne;
    private Vector2 direction;
    private float speed;

    private float airActiveTime = 0.15f;
    private float airCooldown = 0.15f;

    private float activeTime = 0.15f;
    private float cooldown = 0.15f;
    private float activeTimer = 0.0f;

    private float orignalGravScale;

    public DashState(Player playerInstance, StateMachine<Player> fsm, float dir, bool airborne) : base(playerInstance, fsm)
    {
        player = playerInstance;
        this.airborne = airborne;
        this.direction = Vector2.right * dir;
        if(airborne)
            this.speed = player.airDashSpeed;
        else
            this.speed = player.dashSpeed;
    }

    override public void Enter()
    {
        Debug.Log("I did a dash, add aethetics/balance this");
        Debug.Log("Inputs are jank as heeeck");

        if (airborne)
        {
            orignalGravScale = player.selfBody.gravityScale;
            player.airdashCount++;
            player.selfBody.gravityScale = 0.0f;
        }


        //Keeping track of player status
        if (airborne)
            player.status = PlayerStatus.AirDashing;
        else
            player.status = PlayerStatus.Dashing;

        player.locked = true;
    }

    override public void Execute()
    {
        //Keeping track of player status
        if (airborne)
        {
            if (activeTimer < airActiveTime)
                player.status = PlayerStatus.AirDashing;
            else
                player.status = PlayerStatus.Air;
        }
        else
            player.status = PlayerStatus.Dashing;

        activeTimer += Time.deltaTime;

        if (airborne)
        {
            if (activeTimer > airActiveTime + airCooldown)
            {
                player.locked = false;
                if (Controls.attackInputDown(player))
                {
                    player.PerformAction(Action.AirAttack);
                }
            }
        }
        else
        {
            if (activeTimer > activeTime + cooldown)
            {
                player.locked = false;
                if (!player.isCrouching)
                    player.PerformAction(Action.Stand);
                else
                    player.PerformAction(Action.Crouch);
                return;
            }
        }

    }

    override public void FixedExecute()
    {
        if (airborne)
        {
            if (activeTimer < airActiveTime)
                player.selfBody.velocity = player.dashSpeed * direction;
            else if(activeTimer < airActiveTime + airCooldown)
            {
                speed -= 0.95f * Time.fixedDeltaTime/ cooldown;
                player.selfBody.velocity = speed * direction;
            }
            else
            {
                player.selfBody.gravityScale = orignalGravScale;
            }

            //Controls transitioning back to the ground state
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
        else
        {
            if (activeTimer < activeTime)
                player.selfBody.velocity = speed * direction;
            else
            {
                speed *= 0.5f;
                player.selfBody.velocity = speed * direction;
            }
        }
    }

    override public void Exit()
    {
        player.locked = false;
        player.selfBody.gravityScale = orignalGravScale;
    }
}
