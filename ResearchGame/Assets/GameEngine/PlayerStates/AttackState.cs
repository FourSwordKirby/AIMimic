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

    private Vector3 startPosition;
    private Vector3 endPosition;

    public AttackState(Player playerInstance, StateMachine<Player> fsm, int comboCount = 0) : base(playerInstance, fsm)
    {
        player = playerInstance;
        player.comboCount = comboCount;

        meleeHitbox = player.hitboxManager.getHitbox("MeleeHitbox").gameObject;

        attackDistance = 0.55f;

        startup = 0.05f;
        duration = 0.05f;
        endlag = 0.25f;

        timer = 0;
    }

    override public void Enter()
    {
        GameManager.instance.playSound("PunchWiff");

        this.player.selfBody.drag = 20.0f;
        if (!player.isCrouching)
        {
            startPosition = Vector3.zero;
            endPosition = player.facingDirection * 1.375f * attackDistance;
        }
        else
        {
            startPosition = Vector3.down * 0.25f;
            endPosition = startPosition + player.facingDirection * attackDistance;
        }


        if (player.comboCount >= 2)
        {
            player.chainable = false;
            meleeHitbox.GetComponent<Hitbox>().knockdown = true;
        }

        if (player.isCrouching)
            meleeHitbox.GetComponent<Hitbox>().type = Hitbox.hitType.low;
        else
            meleeHitbox.GetComponent<Hitbox>().type = Hitbox.hitType.mid;
        meleeHitbox.transform.localPosition = startPosition;
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white;
    }

    override public void Execute()
    {
        if(player.chainable)
        {
            if (Controls.attackInputDown(player))
                player.Attack();
        }

        timer += Time.deltaTime;
        if (timer < startup)
        {
            meleeHitbox.transform.localPosition = Vector3.Lerp(startPosition, endPosition, timer / startup);
        }
        else if (timer < startup + duration)
        {
            if (timer - Time.deltaTime < startup)
                player.hitboxManager.activateHitBox("MeleeHitbox");

            meleeHitbox.transform.localPosition = endPosition;
        }
        else if (timer < startup + duration + endlag)
        {
            if (timer - Time.deltaTime < startup + duration)
                player.hitboxManager.deactivateHitBox("MeleeHitbox");

            meleeHitbox.transform.localPosition = Vector3.Lerp(endPosition, startPosition, (timer-startup-duration) / endlag);
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
        this.player.selfBody.drag = 0.0f;
        player.chainable = false;

        meleeHitbox.GetComponent<Hitbox>().knockdown = false;
        player.hitboxManager.deactivateHitBox("MeleeHitbox");
        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.clear;
    }
}
