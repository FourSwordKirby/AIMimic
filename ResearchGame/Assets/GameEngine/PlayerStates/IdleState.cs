using UnityEngine;
using System.Collections;

public class IdleState : State<Player> {

    private Player player;

    public IdleState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
    }

    override public void Enter()
    {
        this.player.selfBody.drag = 20.0f;
        this.player.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        this.player.selfBody.angularVelocity = 0;
    }

    override public void Execute()
    {
        Parameters.InputDirection dir = Controls.getInputDirection(player);

        //Change this to be a jump button
        if (Controls.jumpInputDown(player))
        {
            if (dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
                player.performAction(Action.JumpRight);
            else if (dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
                player.performAction(Action.JumpLeft);
            else
                player.performAction(Action.JumpNeutral);
            return;
        }

        if (Controls.attackInputDown(player))
        {
            player.performAction(Action.Attack);
        }

        if (!player.AIControlled)
        {
            if (Controls.shieldInputHeld(player))
            {
                player.performAction(Action.Block);
            }

            if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
            {
                if(!player.isCrouching)
                    player.performAction(Action.Crouch);
                return;
            }
            else
            {
                if (player.isCrouching)
                    player.performAction(Action.Stand);
            }
        }

        if (dir != Parameters.InputDirection.None)
        {
            if (dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.SE)
                player.performAction(Action.WalkRight);
            else if (dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.SW)
                player.performAction(Action.WalkLeft);
            return;
        }
    }

    override public void FixedExecute()
    {
        //if(!player.isCrouching)
        //    this.player.selfBody.velocity = Vector2.zero;
    }

    override public void Exit()
    {
        this.player.selfBody.drag = 0.0f;
    }
}
