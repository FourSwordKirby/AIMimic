using UnityEngine;
using System.Collections;

public class MovementState : State<Player> {

    private Player player;
    private Vector3 targetLocation;

    //Used primarily by the datarecorder
    public Vector3 movementVector;

    //TODO: INPUT BUFFER FOR NEXT MOVE
    private Action nextAction = Action.Stand;


    public MovementState(Player playerInstance, StateMachine<Player> fsm, float dir)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
        movementVector = Vector3.right * (int)dir * player.movementSpeed;
    }

    override public void Enter()
    {
        //Vector2 movementVector = Vector2.right * moveDir * player.movementSpeed;
        player.selfBody.velocity = movementVector;
    }

    override public void Execute()
    {
        //player.transform.position += movementVector * 1 / Application.targetFrameRate;

        Parameters.InputDirection dir = Controls.getInputDirection(player);
        if (Controls.jumpInputHeld(player))
        {
            if (dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
                player.PerformAction(Action.JumpRight);
            else if (dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
                player.PerformAction(Action.JumpLeft);
            else
                player.PerformAction(Action.JumpNeutral);
        }

        if (Controls.attackInputDown(player))
        {
            player.PerformAction(Action.Attack);
        }

        if (!player.AIControlled)
        {
            if (Controls.shieldInputHeld(player))
            {
                player.PerformAction(Action.StandBlock);
                return;
            }
        }

        if (!(dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E))
        {
            if (!player.AIControlled)
            {
                if (dir == Parameters.InputDirection.None)
                    player.PerformAction(Action.Stand);
                else
                    player.PerformAction(Action.Crouch);
            }
        }
    }

    override public void FixedExecute()
    {
            player.selfBody.velocity = movementVector;
    }

    override public void Exit()
    {
        return;
    }
}
