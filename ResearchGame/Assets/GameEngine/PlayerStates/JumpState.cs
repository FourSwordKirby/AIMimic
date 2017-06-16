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
        float displacement = targetLocation.x - playerInstance.transform.position.x;

        player = playerInstance;
        this.targetLocation = new Vector3(Mathf.Clamp(targetLocation.x, - 10, 10), Mathf.Clamp(targetLocation.y, 0, 10), 0);
    }

    override public void Enter()
    {
        GameManager.instance.playSound("Jump");
        float displacement = targetLocation.x - player.transform.position.x;
        if (displacement == 0)
        {
            player.selfBody.velocity = getJumpVelocity(player.neutralJumpHeight, 0, 0.7f);
        }
        else if (displacement > 0)
        {
            player.selfBody.velocity = getJumpVelocity(player.directionJumpHeight, displacement, 0.7f);
        }
        else
        {   
            player.selfBody.velocity = getJumpVelocity(player.directionJumpHeight, displacement, 0.7f);
        }
        jumpVector = player.selfBody.velocity;
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

    public float gravScale;
    /*error with collision boxes puts player in air state when he actually isn't*/
    override public void Execute()
    {
        if (Controls.attackInputDown(player))
        {
            player.PerformAction(Action.AirAttack);
        }
    }

    override public void FixedExecute()
    {
        if (player.grounded && player.selfBody.velocity.y <= 0)
        {
            Parameters.InputDirection dir = Controls.getInputDirection(player);

            //We don't call preformAction because it's not voluntarily done on the part of the player
            if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
                player.PerformAction(Action.Crouch);
            else
                player.PerformAction(Action.Idle);
            return;
        }
    }

    override public void Exit()
    {
    }
}
