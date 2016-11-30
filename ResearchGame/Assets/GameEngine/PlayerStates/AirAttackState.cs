using UnityEngine;
using System.Collections;

public class AirAttackState : State<Player>
{
    private Player player;
    private GameObject meleeHitbox;

    private float attackDistance;

    public float startup;
    public float duration;
    public float endlag;

    private float timer;

    public AirAttackState(Player playerInstance, StateMachine<Player> fsm) : base(playerInstance, fsm)
    {
        player = playerInstance;
        meleeHitbox = player.hitboxManager.getHitbox("AirMeleeHitbox").gameObject;

        attackDistance = 1.55f;

        startup = 0.1f;
        duration = 0.2f;
        endlag = 0.1f;

        timer = 0;
    }

    override public void Enter()
    {
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white;
    }

    override public void Execute()
    {
        //ANIMATE THE HITBOX MOVING
        timer += Time.deltaTime;
        if (timer < startup)
        {
            meleeHitbox.transform.localScale = Vector3.Lerp(new Vector3(0.5f, 0.5f, 1), new Vector3(attackDistance, attackDistance, 1), timer / startup);
        }
        else if (timer < startup + duration)
        {
            meleeHitbox.transform.localScale = Vector3.Lerp(new Vector3(0.5f, 0.5f, 1), new Vector3(attackDistance, attackDistance, 1), timer / startup);
            player.hitboxManager.activateHitBox("AirMeleeHitbox");
        }
        else if (timer < startup + duration + endlag)
        {
            meleeHitbox.transform.localScale = Vector3.Lerp(new Vector3(0.5f, 0.5f, 1), new Vector3(1.5f, 1.5f, 1), ((startup + duration + endlag)-timer) / endlag);
            player.hitboxManager.activateHitBox("AirMeleeHitbox");
        }
        else
        {
            meleeHitbox.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        player.hitboxManager.deactivateHitBox("AirMeleeHitbox");
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
    }
}
