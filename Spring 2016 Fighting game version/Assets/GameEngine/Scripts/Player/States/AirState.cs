using UnityEngine;
using System.Collections;

public class AirState : State<Player>
{
    private Player player;
    private Vector2 movementInputVector;

    public AirState(Player playerInstance, StateMachine<Player> fsm, Vector2 movementInputVector)
        : base(playerInstance, fsm)
    {
        this.movementInputVector = movementInputVector;
        player = playerInstance;
    }

    override public void Enter()
    {
        if(player.grounded)
            player.selfBody.velocity = new Vector2(player.selfBody.velocity.x, player.jumpHeight);
        player.grounded = false;
        return;
    }


    /*error with collision boxes puts player in air state when he actually isn't*/
    override public void Execute()
    {
        //Might want to change this stuff later to include transition states
        //Check if the player is grounded.
        if (player.grounded)
        {
            player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
            return;
        }

        //Doing double jumps
        if (Controls.jumpInputDown(player) && player.airJumps < player.maxAirJumps)
        {
            player.airJumps++;
            player.selfBody.velocity = new Vector2(player.selfBody.velocity.x, player.jumpHeight);
            return;
        }

        //Doing air supers
        if (Controls.superInputDown(player))
        {
            player.ActionFsm.ChangeState(new DownSuperState(player, player.ActionFsm));
        }

        //Temporary measures until we get more animations.
        //if (movementInputVector.x != 0)
        //    player.anim.SetFloat("DirX", movementInputVector.x / Mathf.Abs(movementInputVector.x));
        //player.anim.SetFloat("DirY", Mathf.Ceil(Parameters.getVector(player.direction).y));
    }

    override public void FixedExecute()
    {
        player.selfBody.velocity = new Vector2(movementInputVector.x * player.airMovementSpeed, player.selfBody.velocity.y);
    }

    override public void Exit()
    {
        player.airJumps = 0;
    }
}
