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
        player.anim.SetFloat("MoveSpeed", 0.0f);
    }

    override public void Execute()
    {
        Vector2 movementInputVector = Controls.getDirection(player);
        //Might want to change this stuff later to include transition states
        //Moving
        if (movementInputVector.x != 0)
        {
            player.direction = Parameters.vectorToDirection(movementInputVector);

            player.ActionFsm.ChangeState(new MovementState(player, player.ActionFsm));
            return;
        }

        //Jumping
        if (Controls.jumpInputDown(player))
        {
            player.ActionFsm.ChangeState(new AirState(player, player.ActionFsm, Vector2.up));
            return;
        }

        if (Controls.superInputDown(player))
        {
            player.ActionFsm.ChangeState(new DownSuperState(player, player.ActionFsm));
        }

        if (Controls.shieldInputDown(player))
        {
            player.ActionFsm.ChangeState(new ShieldState(player, player.ActionFsm));
        }
    }

    override public void FixedExecute(){    }

    override public void Exit(){    }
}
