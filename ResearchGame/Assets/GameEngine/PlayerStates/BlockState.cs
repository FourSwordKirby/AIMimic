using UnityEngine;
using System.Collections;

public class BlockState :  State<Player> {

    private Player player;

    public BlockState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
    }

    override public void Enter()
    {
        this.player.selfBody.drag = 20.0f;
        player.isBlocking = true;
        player.shield.SetActive(true);
        //Draw a visual indicator for the player that they are blocking, the current shield sprite is eh
        //The shield doesn't really do anything, it does faciliate effects though
    }

    override public void Execute()
    {
        Parameters.InputDirection dir = Controls.getInputDirection(player);

        if (!player.AIControlled)
        {
            if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
            {
                if (!player.isCrouching)
                    player.PerformAction(Action.Crouch);
            }
            else
            {
                if (player.isCrouching)
                    player.PerformAction(Action.Stand);
            }

            if (!Controls.shieldInputHeld(player))
            {
                player.PerformAction(Action.Idle);
                return;
            }
        }

        if(player.isCrouching)
        {
            player.shield.transform.localPosition = 0.25f * Vector3.up + 0.25f * player.facingDirection.x * Vector3.right;
            player.shield.transform.localScale = Vector3.one * 0.5f + Vector3.up * 0.5f;
            player.shield.transform.localScale *= 0.75f;
        }
        else
        {
            player.shield.transform.localPosition = 0.5f * Vector3.up + 0.25f * player.facingDirection.x * Vector3.right;
            player.shield.transform.localScale = Vector3.one * 0.5f;
        }

        if (Controls.jumpInputDown(player))
        {
            Action chosenAction;
            if (dir == Parameters.InputDirection.NE || dir == Parameters.InputDirection.E || dir == Parameters.InputDirection.SE)
                chosenAction = Action.JumpRight;
            else if (dir == Parameters.InputDirection.NW || dir == Parameters.InputDirection.W || dir == Parameters.InputDirection.SW)
                chosenAction = Action.JumpLeft;
            else
                chosenAction = Action.JumpNeutral;
            player.PerformAction(chosenAction);
            return;
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        this.player.selfBody.drag = 0.0f;
        player.isBlocking = false;
        player.shield.SetActive(false);
    }
}
