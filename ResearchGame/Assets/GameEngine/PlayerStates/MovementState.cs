using UnityEngine;
using System.Collections;

public class MovementState : State<Player> {

    private Player player;
    private Vector3 targetLocation;

    public MovementState(Player playerInstance, StateMachine<Player> fsm, Vector3 targetLocation)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
        this.targetLocation = new Vector3(Mathf.Clamp(targetLocation.x, - 10, 10), Mathf.Clamp(targetLocation.y, 0, 10), 0);
    }

    override public void Enter()
    {
        Vector2 movementVector = (targetLocation - player.transform.position).normalized * player.movementSpeed;
        player.selfBody.velocity = movementVector;
        Debug.Log(player.name + "moving");
    }

    override public void Execute()
    {
        //TODO: allow for buffers into jumps etc.
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
        Debug.Log(player.name + "not moving");
        return;
    }
}
