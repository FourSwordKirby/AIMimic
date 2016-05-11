using UnityEngine;
using System.Collections;

public class RollStopState : State<Player> {

    private Player player;

    private float duration;

    private const float startup_duration = 0.1f; //Around 6 frames for a60 fps game?

    public RollStopState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        duration = startup_duration;
        player = playerInstance;
    }

    override public void Enter()
    {
        Debug.Log("entered Roll Stop state");
        return;
    }

    override public void Execute()
    {
        duration -= Time.deltaTime;
        if (duration < 0)
            player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));

        player.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        Debug.Log("exited Roll Stop state");
        return;
    }



}
