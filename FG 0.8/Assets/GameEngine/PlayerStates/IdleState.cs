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
        this.player.selfBody.drag = 200.0f;
        this.player.selfBody.velocity = Vector3.zero;
        this.player.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        this.player.selfBody.angularVelocity = 0;

        //Keeping track of player status
        if (!player.isCrouching)
            player.status = PlayerStatus.Stand;
        else
            player.status = PlayerStatus.Crouch;
    }

    override public void Execute()
    {
        Parameters.InputDirection dir = Controls.getInputDirection(player);
        if (Controls.jumpInputDown(player))
        {
            player.EndAction();
            if (dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
                player.PerformAction(Action.JumpRight);
            else if (dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
                player.PerformAction(Action.JumpLeft);
            else
                player.PerformAction(Action.JumpNeutral);
            return;
        }

        if (Controls.attackInputDown(player))
        {
            player.EndAction();
            if (!player.isCrouching)
            {
                if (dir == Parameters.InputDirection.N || dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.NW)
                    player.PerformAction(Action.DP);
                else if ((player.facingDirection.x > 0 && dir == Parameters.InputDirection.W)
                    || (player.facingDirection.x <= 0 && dir == Parameters.InputDirection.E))
                    player.PerformAction(Action.Overhead);
                else
                    player.PerformAction(Action.Attack);
            }
            else
                player.PerformAction(Action.LowAttack);
            return;
        }


        if (Controls.dashInputDown(player))
        {
            if (dir == Parameters.InputDirection.W)
            {
                player.EndAction();
                player.PerformAction(Action.DashLeft);
            }
            else if (dir == Parameters.InputDirection.E)
            {
                player.EndAction();
                player.PerformAction(Action.DashRight);
            }
        }

        if (!player.AIControlled)
        {
            if (Controls.shieldInputHeld(player))
            {
                player.EndAction();
                if (!player.isCrouching)
                    player.PerformAction(Action.StandBlock);
                else
                    player.PerformAction(Action.CrouchBlock);
            }

            if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
            {
                if(!player.isCrouching)
                {
                    player.EndAction();
                    player.PerformAction(Action.Crouch);
                }
                return;
            }
            else if (dir == Parameters.InputDirection.None)
            {
                if (player.isCrouching)
                {
                    player.EndAction();
                    player.PerformAction(Action.Stand);
                }
            }
        }

        if (dir != Parameters.InputDirection.None)
        {
            if (dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.SE)
            {
                player.EndAction();
                player.PerformAction(Action.WalkRight);
            }
            else if (dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.SW)
            {
                player.EndAction();
                player.PerformAction(Action.WalkLeft);
            }
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
