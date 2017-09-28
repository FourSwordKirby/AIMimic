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
        this.player.selfBody.drag = 200.0f;
        player.isBlocking = true;
        player.shield.SetActive(true);

        //Keeping track of player status
        if (!player.isCrouching)
            player.status = PlayerStatus.Highblock;
        else
            player.status = PlayerStatus.Lowblock;
    }

    override public void Execute()
    {
        //Keeping track of player status
        if (!player.isCrouching)
            player.status = PlayerStatus.Highblock;
        else
            player.status = PlayerStatus.Lowblock;

        Parameters.InputDirection dir = Controls.getInputDirection(player);

        if (!player.AIControlled)
        {
            if (dir == Parameters.InputDirection.S || dir == Parameters.InputDirection.SW || dir == Parameters.InputDirection.SE)
            {
                if (!player.isCrouching)
                    player.PerformAction(Action.CrouchBlock);
            }
            else
            {
                if (player.isCrouching)
                    player.PerformAction(Action.StandBlock);
            }

            if (!Controls.shieldInputHeld(player))
            {
                if (!player.isCrouching)
                    player.PerformAction(Action.Stand);
                else
                    player.PerformAction(Action.Crouch);
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
