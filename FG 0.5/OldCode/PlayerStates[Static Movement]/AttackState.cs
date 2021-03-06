﻿using UnityEngine;
using System.Collections;

public class AttackState : State<Player>
{
    private Player player;
    private GameObject meleeHitbox;

    private float attackDistance;

    public float startup;
    public float duration;
    public float endlag;
    
    private float timer;

    public AttackState(Player playerInstance, StateMachine<Player> fsm) : base(playerInstance, fsm)
    {
        player = playerInstance;
        meleeHitbox = player.hitboxManager.getHitbox("MeleeHitbox").gameObject;

        attackDistance = 0.55f;

        startup = 0.05f;
        duration = 0.1f;
        endlag = 0.05f;

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
            meleeHitbox.transform.position += player.facingDirection * attackDistance * (Time.deltaTime/startup);
        }
        else if (timer < startup + duration)
        {
            if(timer <= Time.deltaTime +startup)
                meleeHitbox.transform.localPosition = player.facingDirection * attackDistance;
            player.hitboxManager.activateHitBox("MeleeHitbox");
        }
        else if (timer < startup + duration + endlag)
        {
            meleeHitbox.transform.position -= player.facingDirection * attackDistance * (Time.deltaTime / endlag);
            player.hitboxManager.activateHitBox("MeleeHitbox");
        }
        else
        {
            meleeHitbox.transform.localPosition = Vector2.zero;
            player.ActionFsm.ChangeState(new IdleState(player, player.ActionFsm));
        }
    }

    override public void FixedExecute()
    {
    }

    override public void Exit()
    {
        player.hitboxManager.deactivateHitBox("MeleeHitbox");
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
    }
}
