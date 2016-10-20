using UnityEngine;
using System.Collections;

public class JumpState : State<Player>
{
    private Player player;
    public Vector3 targetLocation { get; private set; }

    //Used primarily by the datarecorder
    public int jumpDir;

    public JumpState(Player playerInstance, StateMachine<Player> fsm, Vector3 targetLocation)
        : base(playerInstance, fsm)
    {
        float displacement = targetLocation.x - playerInstance.effectivePosition.x;
        jumpDir = displacement < 0 ? -1 : displacement > 0 ? 1: 0;

        player = playerInstance;
        this.targetLocation = new Vector3(Mathf.Clamp(targetLocation.x, - 10, 10), Mathf.Clamp(targetLocation.y, 0, 10), 0);
    }

    override public void Enter()
    {
        float displacement = targetLocation.x - player.effectivePosition.x;
        if (displacement == 0)
        {
            player.selfBody.velocity = getJumpVelocity(2, 0, 0.7f);
        }
        else if (displacement > 0)
        {
            player.selfBody.velocity = getJumpVelocity(1.5f, displacement, 0.7f);
        }
        else
        {   
            player.selfBody.velocity = getJumpVelocity(1.5f, displacement, 0.7f);
        }
        player.grounded = false;
        return;
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
        //Check if the player hit the ground etc.
    }

    override public void FixedExecute()
    {
        if (player.transform.position.y < Parameters.positionLeeway && player.selfBody.velocity.y < 0)
        {
            player.Idle();
            return;
        }
    }

    override public void Exit()
    {
        player.selfBody.velocity = Vector2.zero;
        player.grounded = true;
    }
}
