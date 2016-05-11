using UnityEngine;
using System.Collections;

public class ParryState : State<Player>
{

    private Player player;
    private float baseKnockbackThreshold;

    private float parryMovementSpeed;

    private float duration;

    private const float parry_duration = 1.0f; //The required amount of time you have to hold the parry
    private const float parry_start = 0.1f; //When the parry frames actually start
    private const float parry_end = 0.6f; //When the parry frames end


    public ParryState(Player playerInstance, StateMachine<Player> fsm)
        : base(playerInstance, fsm)
    {
        duration = 0;
        player = playerInstance;
        baseKnockbackThreshold = player.knockdownThreshold;
    }

    override public void Enter()
    {
        Debug.Log("entered parry state");
        player.anim.SetTrigger("Parry");
        player.anim.SetBool("Blocking", true);
        player.knockdownThreshold = 0.0f;

        player.hitboxManager.activateHitBox("Parrybox");

        parryMovementSpeed = player.movementSpeed * 0.2f;
    }

    override public void Execute()
    {
        duration += Time.deltaTime;
        if (duration > parry_duration)
        {
        }

        if (duration > parry_start && duration < parry_end)
            player.status = Parameters.PlayerStatus.Counter;
        else
            player.status = Parameters.PlayerStatus.Default;
    }

    override public void FixedExecute()
    {
        if (duration > parry_duration)
            player.selfBody.velocity = Parameters.VectorToDir(Controls.getInputDirection(player)) * parryMovementSpeed;
    }

    override public void Exit()
    {
        player.anim.SetBool("Blocking", false);
        player.status = Parameters.PlayerStatus.Default;
        player.knockdownThreshold = baseKnockbackThreshold;

        player.hitboxManager.deactivateHitBox("Parrybox");
    }
}