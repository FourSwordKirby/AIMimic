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
            player.Jump(dir);
            return;
        }

        if (Controls.attackInputDown(player))
        {
            player.Attack();
        }

        if (!player.AIControlled)
        {
            if (Controls.shieldInputHeld(player))
            {
                player.Block();
            }

            if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
            {
                player.isCrouching = true;
                return;
            }
            else
            {
                player.isCrouching = false;
            }
        }

        if (dir != Parameters.InputDirection.None)
        {
            player.Walk(dir);
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
