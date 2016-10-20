using UnityEngine;
using System.Collections;

public class MovementState : State<Player> {

    private Player player;
    private Vector3 targetLocation;

    //Used primarily by the datarecorder
    public int moveDir;

    public MovementState(Player playerInstance, StateMachine<Player> fsm, Vector3 targetLocation)
        : base(playerInstance, fsm)
    {
        float displacement = targetLocation.x - playerInstance.effectivePosition.x;
        moveDir = displacement < 0 ? -1 : displacement > 0 ? 1 : 0;

        player = playerInstance;
        this.targetLocation = new Vector3(Mathf.Clamp(targetLocation.x, - 10, 10), Mathf.Clamp(targetLocation.y, 0, 10), 0);
    }

    override public void Enter()
    {
        Vector2 movementVector = (targetLocation - player.transform.position).normalized * player.movementSpeed;
        player.selfBody.velocity = movementVector;
    }

    override public void Execute()
    {
        //TODO: allow for buffers into jumps etc.
        Parameters.InputDirection dir = Controls.getInputDirection(player);
        //Change this to be a jump button
        if (Controls.jumpInputDown(player))
        {
            player.Jump(dir);
            return;
        }

        if (dir != Parameters.InputDirection.None)
        {
            player.Walk(dir);
            return;
        }
    }

    override public void FixedExecute()
    {
        if (Mathf.Sign((targetLocation - player.transform.position).x) != Mathf.Sign(player.selfBody.velocity.x))
        {
            targetLocation = player.effectivePosition;
        }
        if (Vector2.Distance(player.transform.position, targetLocation) < Parameters.positionLeeway)
        {
            player.selfBody.velocity = Vector2.zero;
            player.transform.position = targetLocation;
            player.Idle();
        }
    }

    override public void Exit()
    {
        return;
    }
}
