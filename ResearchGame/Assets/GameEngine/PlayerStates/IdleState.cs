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
    }

    override public void FixedExecute()
    {
        if (Vector2.Distance(player.transform.position, player.effectivePosition) > Parameters.positionLeeway)
        {
            Vector2 movementVector = (player.effectivePosition - new Vector2(player.transform.position.x, 0)).normalized;
            player.selfBody.velocity = movementVector;
        }
        else 
        {
            //TODO: Doesn't actually work currently, jump ins are bugged due to this. Probably because floating points and edge collisions
            player.transform.position = player.effectivePosition;
        }
    }

    override public void Exit(){    }
}
