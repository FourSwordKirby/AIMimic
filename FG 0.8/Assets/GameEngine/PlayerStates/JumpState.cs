using UnityEngine;
using System.Collections;

public class JumpState : State<Player>
{
    private Player player;
    public Vector3 targetLocation { get; private set; }

    //Used primarily by the datarecorder
    public Vector3 jumpVector;

    public JumpState(Player playerInstance, StateMachine<Player> fsm, Vector3 targetLocation)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
        this.targetLocation = new Vector3(Mathf.Clamp(targetLocation.x, -10, 10), Mathf.Clamp(targetLocation.y, 0, 10), 0);
    }

    //Adjust the dash and airdash distances later ;_;
    override public void Enter()
    {
        if (!player.grounded)
        {
            return;
        }

        GameManager.instance.PlaySound("Jump");
        float displacement = targetLocation.x - player.transform.position.x;
        if (displacement == 0)
        {
            jumpVector = getJumpVelocity(player.neutralJumpHeight, 0, 0.65f);
        }
        else if (displacement > 0)
        {
            jumpVector = getJumpVelocity(player.directionJumpHeight, displacement, 0.65f);
        }
        else
        {
            jumpVector = getJumpVelocity(player.directionJumpHeight, displacement, 0.65f);
        }

        player.selfBody.velocity = jumpVector;
        player.grounded = false;

        player.selfBody.drag = 1.0f;
    }

    public Vector2 getJumpVelocity(float height, float distance, float time)
    {
        float xVel = distance / time;
        float yVel = 2 * height/(0.5f * time);
        player.selfBody.gravityScale = -yVel / ((0.5f * time) * Physics2D.gravity.y);

        //x = vt + 1/2(a)t^2
        //a = -v/t

        //x = vt - 1/2(v)t
        //x = 1/2 vt
        return new Vector2(xVel, yVel);
    }
    
    /*error with collision boxes puts player in air state when he actually isn't*/
    override public void Execute()
    {
        if (Controls.attackInputDown(player))
        {
            player.PerformAction(Action.AirAttack);
        }

        if (Controls.dashInputDown(player))
        {
            Parameters.InputDirection dir = Controls.getInputDirection(player);

            if (dir == Parameters.InputDirection.W)
                player.PerformAction(Action.AirdashLeft);
            else if (dir == Parameters.InputDirection.E)
                player.PerformAction(Action.AirdashRight);
            else
            {
                if(player.facingDirection.x > 0)
                    player.PerformAction(Action.AirdashRight);
                else
                    player.PerformAction(Action.AirdashLeft);
            }
        }
    }

    override public void FixedExecute()
    {
        if (jumpVector == Vector3.zero)
            return;

        jumpVector += Time.fixedDeltaTime * player.selfBody.gravityScale * (Vector3)Physics2D.gravity;
        player.selfBody.velocity = jumpVector;

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

    override public void Exit()
    {
    }
}
