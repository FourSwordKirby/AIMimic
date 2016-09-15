using UnityEngine;
using System.Collections;

public class RespawnState : State<Player>
{
    private Player player;

    private float respawnTime = 2.0f;

    public RespawnState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
        player.selfBody.isKinematic = true;
        player.hitboxManager.deactivateAllHitboxes();
    }

    override public void Enter()
    {
    }

    override public void Execute()
    {
        respawnTime -= Time.deltaTime;
        if (respawnTime < 0)
        {
            player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        player.selfBody.isKinematic = false;
        player.hitboxManager.deactivateAllHitboxes();
    }
}
