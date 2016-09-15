using UnityEngine;
using System.Collections;

public class ShieldState :  State<Player> {

    private Player player;

    public ShieldState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        player = playerInstance;
    }

    override public void Enter()
    {
        player.shield.RaiseShield();
        return;
    }

    override public void Execute()
    {
        if (player.shield.currentShieldSize < 0)
        {
            player.ActionFsm.ChangeState(new HitState(player, 0.01f, 3.0f, Vector2.zero, player.ActionFsm));
        }

        if (!Controls.shieldInputHeld(player))
        {
            player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        player.shield.LowerShield();
        return;
    }
}
