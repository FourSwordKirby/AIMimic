using UnityEngine;
using System.Collections;

public class AttackState : State<Player>
{
    private Player player;
    private GameObject meleeHitbox;

    private float attackDistance;

    public float startup;
    public float duration;
    public float endlag;
    
    private float frameCounter;

    private Vector3 startPosition;
    private Vector3 endPosition;

    public AttackState(Player playerInstance, StateMachine<Player> fsm, int comboCount = 0) : base(playerInstance, fsm)
    {
        player = playerInstance;
        player.comboCount = comboCount;

        meleeHitbox = player.hitboxManager.getHitbox("MeleeHitbox").gameObject;

        attackDistance = 0.4f;

        startup = 0.05f * Application.targetFrameRate;
        duration = 0.05f * Application.targetFrameRate;
        endlag = 0.25f * Application.targetFrameRate;

        frameCounter = 0;
    }

    override public void Enter()
    {
        GameManager.instance.PlaySound("PunchWiff");

        this.player.selfBody.drag = 200.0f;
        this.player.selfBody.velocity = Vector2.zero;

        if (player.isCrouching)
        {
            startPosition = Vector3.down * 0.3f;
            endPosition = startPosition + player.facingDirection * attackDistance;
            meleeHitbox.transform.localScale = Vector3.right + Vector3.up * 0.125f + Vector3.forward;
        }
        else
        {
            startPosition = Vector3.zero;
            endPosition = player.facingDirection * 1.375f * attackDistance;
            meleeHitbox.transform.localScale = Vector3.right + Vector3.up * 0.25f + Vector3.forward;
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

        //Player state managing
        player.locked = true;

        //Keeping track of player status
        if (!player.isCrouching)
            player.status = PlayerStatus.StandAttack;
        else
            player.status = PlayerStatus.LowAttack;
    }

    override public void Execute()
    {
        //Keeping track of player status
        if (frameCounter < startup + duration)
        {
            if (!player.isCrouching)
                player.status = PlayerStatus.StandAttack;
            else
                player.status = PlayerStatus.LowAttack;
        }
        else
            player.status = PlayerStatus.Recovery;

        meleeHitbox.GetComponent<SpriteRenderer>().color = Color.white;
        if (player.chainable)
        {
            player.locked = false;
            if (Controls.attackInputDown(player))
            {
                if (player.isCrouching)
                    player.PerformAction(Action.LowAttack);
                else
                    player.PerformAction(Action.Attack);
            }
        }

        frameCounter++;
        if (frameCounter < startup)
        {
            meleeHitbox.transform.localPosition = Vector3.Lerp(startPosition, endPosition, frameCounter / startup);
        }
        else if (frameCounter < startup + duration)
        {
            if (frameCounter - 1 < startup)
                player.hitboxManager.activateHitBox("MeleeHitbox");

            meleeHitbox.transform.localPosition = endPosition;
        }
        else if (frameCounter - 1 < startup + duration + endlag)
        {
            if (frameCounter - 1 < startup + duration)
                player.hitboxManager.deactivateHitBox("MeleeHitbox");

            meleeHitbox.transform.localPosition = Vector3.Lerp(endPosition, startPosition, (frameCounter - startup - duration) / endlag);
        }
        else
        {
            player.locked = false;
            meleeHitbox.transform.localPosition = Vector2.zero;
            if (!player.isCrouching)
                player.PerformAction(Action.Stand);
            else
                player.PerformAction(Action.Crouch);
            return;
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

        //Player state managing
        player.locked = false;
    }

    override public State<Player> Copy()
    {
        AttackState attackCopy = new AttackState(this.Owner, this.Owner.ActionFsm, player.comboCount);
        attackCopy.frameCounter = frameCounter;
        attackCopy.startPosition = this.startPosition;
        attackCopy.endPosition = this.endPosition;

        attackCopy.player.chainable = player.chainable;
        attackCopy.meleeHitbox.GetComponent<Hitbox>().knockdown = meleeHitbox.GetComponent<Hitbox>().knockdown;
        attackCopy.meleeHitbox.GetComponent<Hitbox>().type = meleeHitbox.GetComponent<Hitbox>().type;
        attackCopy.meleeHitbox.transform.localPosition = meleeHitbox.transform.localPosition;
        return attackCopy;
    }
}
