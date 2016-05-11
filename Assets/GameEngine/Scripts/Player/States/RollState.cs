using UnityEngine;
using System.Collections;

public class RollState : State<Player> {

    private Player player;

    private float duration;

    private const float roll_duration = 0.3f; //Around 6 frames for a 60 fps game?

    public RollState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
    }

    override public void Enter()
    {
        Debug.Log("entered roll state");
        duration = roll_duration;

        player.GetComponent<Rigidbody2D>().velocity = Parameters.VectorToDir(player.direction) * player.rollSpeed;

        return;
    }

    override public void Execute()
    {
        duration -= Time.deltaTime;
        if (duration < 0)
            player.ActionFsm.ChangeState(new RollStopState(player, player.ActionFsm));   
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        Debug.Log("exited movement state");
        return;
    }
}
