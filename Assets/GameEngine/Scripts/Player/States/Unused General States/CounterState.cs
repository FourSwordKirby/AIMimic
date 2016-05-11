using UnityEngine;
using System.Collections;

public class CounterState : State<Player>
{
    private Player player;
    private float baseKnockbackThreshold;

    private float duration;

    private const float counter_duration = 1.0f; //The total amount of time that the counterlasts
    private const float counter_invincibility_duration = 0.6f; //Amount of invuln afforded
    private const float max_teleport_distance = 10.0f;

    private Parameters.InputDirection counterDirection;
    private bool counterAttack;
    private Vector2 teleportLocation;

    public CounterState(Player playerInstance, StateMachine<Player> fsm) : base(playerInstance, fsm)
    {
        duration = 0;
        player = playerInstance;
        baseKnockbackThreshold = player.knockdownThreshold;
    }

    override public void Enter()
    {
        Debug.Log("entered counter state");
        player.status = Parameters.PlayerStatus.Invincible;

        player.anim.SetTrigger("Counter");

        /*used to manage hitboxes*/
        player.GetComponent<Rigidbody2D>().isKinematic = true;
        player.hitboxManager.deactivateHitBox("Hurtbox");

        /*slowing down everything*/
        Time.timeScale = 0.4f;
    }

    override public void Execute()
    {
        /*
        counterAttack = Controls.attackInputHeld(player);

        if (duration < counter_invincibility_duration && counterDirection == Parameters.InputDirection.Stop)
        {
            counterDirection = Controls.getInputDirection(player);
            teleportLocation = GameManager.getOpenLocation(counterDirection, player.transform.position, max_teleport_distance);

            if (counterDirection != Parameters.InputDirection.Stop)
            {
                player.direction = Parameters.getOppositeDirection(counterDirection);
                player.anim.SetFloat("DirX", Mathf.Ceil(Parameters.getVector(player.direction).x));
                player.anim.SetFloat("DirY", Mathf.Ceil(Parameters.getVector(player.direction).y));
            }
        }
         */

        //I really want to use delegates so that on hit's functionality is replaced by my counter state stuff temporarily
        duration += Time.deltaTime;
        if (duration > counter_invincibility_duration && duration < counter_duration)
            player.status = Parameters.PlayerStatus.Default;
        if (duration > counter_duration)
            player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));

        if (duration > counter_invincibility_duration * 0.8) //basically this is the leeway allowed to decide what to do
        {
            if (player.transform.position.x == teleportLocation.x && player.transform.position.y == teleportLocation.y)
            {
                if (counterAttack)
                {
                    player.ActionFsm.ChangeState(new AttackState(player, player.ActionFsm));
                }
                else if (duration < counter_invincibility_duration)
                    duration = counter_duration * 0.8f;// sort of arbitrary bounds
            }
        }
    }

    override public void FixedExecute()
    {
        if (duration < counter_invincibility_duration)
            player.transform.position = Vector2.MoveTowards(player.transform.position, teleportLocation, max_teleport_distance / 500);
    }

    override public void Exit()
    {
        Debug.Log("exited counter state");
        player.status = Parameters.PlayerStatus.Default;

        /*used to manage hitboxes*/
        player.GetComponent<Rigidbody2D>().isKinematic = false;
        player.hitboxManager.activateHitBox("Hurtbox");

        /*back to normal*/
        Time.timeScale = 1.0f;

        return;
    }
}