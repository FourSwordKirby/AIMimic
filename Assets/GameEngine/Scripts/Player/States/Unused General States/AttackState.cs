using UnityEngine;
using System.Collections;

public class AttackState : State<Player>
{
    private Player player;

    private float duration;
    private float attackVelocity;

    private const float attack_duration = 0.2f; //The total amount of time that the attack


    public AttackState(Player playerInstance, StateMachine<Player> fsm) : base(playerInstance, fsm)
    {
        duration = 0;
        player = playerInstance;
    }

    override public void Enter()
    {
        Debug.Log("entered attack state");
        player.anim.SetTrigger("Attack");

        player.hitboxManager.activateHitBox("SwordHitbox");
        attackVelocity = 2f;//a placeholder for now
    }

    override public void Execute()
    {
        //I really want to use delegates so that on hit's functionality is replaced by my counter state stuff temporarily
        duration += Time.deltaTime;
        if (duration > attack_duration)
            player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
    }

    override public void FixedExecute()
    {
        player.selfBody.velocity = Parameters.VectorToDir(player.direction) * attackVelocity;
    }

    override public void Exit()
    {
        Debug.Log("exited attack state");
        player.selfBody.velocity = Vector2.zero;

        player.hitboxManager.deactivateHitBox("SwordHitbox");
    }
}
