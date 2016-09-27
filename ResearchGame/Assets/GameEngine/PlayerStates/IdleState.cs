using UnityEngine;
using System.Collections;

public class IdleState : State<Player> {

    private Player player;

    public IdleState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
    }

    override public void Enter(){
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

        if (dir != Parameters.InputDirection.None)
        {
            player.Walk(dir);
            return;
        }

        if (Controls.attackInputDown(player))
        {
            player.Attack();
        }

        //Locks the position of the player
        if (Vector2.Distance(player.transform.position, player.effectivePosition) <= Parameters.positionLeeway)
        {
            player.selfBody.velocity = Vector2.zero;
            player.transform.position = player.effectivePosition;
        }
    }

    override public void FixedExecute()
    {
        if (Vector2.Distance(player.transform.position, player.effectivePosition) > Parameters.positionLeeway)
        {
            Vector2 movementVector = (player.effectivePosition - new Vector2(player.transform.position.x, 0)).normalized;
            player.selfBody.velocity = movementVector;
        }
    }

    override public void Exit(){    }
}
