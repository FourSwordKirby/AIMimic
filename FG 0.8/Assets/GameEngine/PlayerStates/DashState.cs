using UnityEngine;
using System.Collections;

public class DashState : State<Player>
{
    private Player player;

    private bool airborne;
    private Vector2 direction;

    private float speed = 20.0f;

    private float minimumActiveTime = 0.2f;
    private float maximumActiveTime = 0.3f;
    private float activeTimer = 0.0f;

    public DashState(Player playerInstance, StateMachine<Player> fsm, float dir, bool airborne) : base(playerInstance, fsm)
    {
        player = playerInstance;
        this.airborne = airborne;
        this.direction = Vector2.right * dir;
    }

    override public void Enter()
    {
        Debug.Log("I did a dash, add aethetics/balance this");
    }

    override public void Execute()
    {
        activeTimer += Time.deltaTime;
        if (activeTimer > maximumActiveTime)
        {
            if(airborne)
                //Create an airstate
                player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
            else
                player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
        }
    }

    override public void FixedExecute()
    {
        if (activeTimer < minimumActiveTime)
        {
            if (airborne)
                player.selfBody.velocity = speed * direction;
            else
                player.selfBody.velocity = 0.5f * speed * direction;
        }
        else
            player.selfBody.velocity *= 0.2f;
    }

    override public void Exit()
    {
    }
}
